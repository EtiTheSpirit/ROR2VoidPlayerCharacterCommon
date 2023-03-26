#pragma warning disable Publicizer001
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BindingFlags = System.Reflection.BindingFlags;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {
	internal static class VoidTeamSurvivorController {

		private static CharacterSpawnCard _nullifierNative;
		private static CharacterSpawnCard _jailerNative;
		private static CharacterSpawnCard _megaCrabNative;
		private static Dictionary<string, CharacterSpawnCard> _replacementLookup = new Dictionary<string, CharacterSpawnCard>();

		internal static void Initialize() {
			On.RoR2.GenericPickupController.AttemptGrant += OnAttemptGrantItem;
			On.RoR2.HoldoutZoneController.CountLivingPlayers += OnCountingLivingPlayers;
			On.RoR2.HoldoutZoneController.CountPlayersInRadius += OnCountingPlayersInRadius;
			On.RoR2.HealthComponent.TakeDamage += OnTakeDamage;
			On.RoR2.CharacterMaster.OnBodyStart += OnCharacterSpawned;
			IL.RoR2.VoidMegaCrabItemBehavior.FixedUpdate += InjectSpawnCardAcquisition;

			_nullifierNative = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Nullifier/cscNullifier.asset").WaitForCompletion();
			_jailerNative = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidJailer/cscVoidJailer.asset").WaitForCompletion();
			_megaCrabNative = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidMegaCrab/cscVoidMegaCrab.asset").WaitForCompletion();

			_replacementLookup["cscNullifierAlly"] = _nullifierNative;
			_replacementLookup["cscVoidJailerAlly"] = _jailerNative;
			_replacementLookup["cscVoidMegaCrabAlly"] = _megaCrabNative;
		}

		#region Zoea Skin
		private static void InjectSpawnCardAcquisition(ILContext il) {
			ILCursor cursor = new ILCursor(il);
			cursor.GotoNext(MoveType.Before, instruction => instruction.MatchLdfld(typeof(VoidMegaCrabItemBehavior).GetField("spawnSelection", BindingFlags.NonPublic | BindingFlags.Instance)));
			cursor.Remove();
			cursor.Remove();
			cursor.GotoNext(MoveType.Before, instruction => instruction.MatchCallvirt(typeof(WeightedSelection<CharacterSpawnCard>).GetMethod("Evaluate")));
			cursor.Remove();
			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate(GetSpawnCard);
		}

		private static CharacterSpawnCard GetSpawnCard(float rng, VoidMegaCrabItemBehavior @this) {
			CharacterSpawnCard card = @this.spawnSelection.Evaluate(rng);
			BodyIndex owner = @this.body.bodyIndex;
			if (UseNativeSkin(owner)) {
				Log.LogTrace($"Spawning a modifiable ally from the Newly Hatched Zoea. This would normally spawn {card.name}...");
				if (_replacementLookup.TryGetValue(card.name, out CharacterSpawnCard replacement)) {
					card = replacement;
					Log.LogTrace($"...But this is being replaced with its native variant, {replacement.name}.");
				} else {
					Log.LogTrace("...And it follow through on that, as there is no known native variant for this entity to swap it with.");
				}
			}
			return card;
		}

		private static bool UseNativeSkin(BodyIndex owner) {
			#pragma warning disable IDE0066 // Not supported in this version of C#.
			switch (Configuration.ZoeaSkinType) {
				case ZoeaSkinBehavior.AlwaysNativeSkin: return true;
				case ZoeaSkinBehavior.AlwaysAllySkin: return false;
				case ZoeaSkinBehavior.IfOwnerIsVoid: return XanVoidAPI.IsVoidSurvivor(owner);
				default: throw new InvalidOperationException("Something set the Zoea skin behavior to an illegal value.");
			}
		}
		#endregion

		#region Team Swap and Damage
		private static void OnCharacterSpawned(On.RoR2.CharacterMaster.orig_OnBodyStart originalMethod, CharacterMaster @this, CharacterBody body) {
			originalMethod(@this, body);
			if (body.isPlayerControlled && Configuration.VoidTeamPlayers && XanVoidAPI.IsVoidSurvivor(body.bodyIndex)) {
				Log.LogTrace("Player is a void survivor. Changing team to Void.");
				body.teamComponent.teamIndex = TeamIndex.Void;
			} else {
				Log.LogTrace($"Body ({body.baseNameToken}) is not allowed to go to Void team (isPlayerControlled={body.isPlayerControlled}, VoidTeamPlayers={Configuration.VoidTeamPlayers}, isVoidSurvivor={XanVoidAPI.IsVoidSurvivor(body.bodyIndex)})");
			}
		}

		private static void OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage originalMethod, HealthComponent @this, DamageInfo damageInfo) {
			if (damageInfo.rejected || !Configuration.VoidTeamPlayers) {
				originalMethod(@this, damageInfo);
				return;
			}

			CharacterBody body = @this.body;
			if (body && body.isPlayerControlled && body.teamComponent && damageInfo.attacker) {
				CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
				if (attackerBody && attackerBody.isPlayerControlled && attackerBody.teamComponent) {
					TeamIndex attackerTeam = attackerBody.teamComponent.teamIndex;
					TeamIndex receiverTeam = body.teamComponent.teamIndex;

					if (attackerTeam == TeamIndex.Void && receiverTeam == TeamIndex.Player) {
						damageInfo.damage *= Configuration.PvPToVoidTeamDamageMult;
					} else if (attackerTeam == TeamIndex.Player && receiverTeam == TeamIndex.Void) {
						damageInfo.damage *= Configuration.PvPToVoidTeamDamageMult;
					}
				}

			}
			originalMethod(@this, damageInfo);
		}
		#endregion

		#region Fix Player Interactions
		private static void OnAttemptGrantItem(On.RoR2.GenericPickupController.orig_AttemptGrant originalMethod, RoR2.GenericPickupController @this, RoR2.CharacterBody body) {
			if (!NetworkServer.active || !body || !body.isPlayerControlled || !body.teamComponent) {
				originalMethod(@this, body);
				return;
			}

			TeamIndex oldIndex = body.teamComponent.teamIndex;
			body.teamComponent._teamIndex = TeamIndex.Player;
			try {
				originalMethod(@this, body);
			} finally {
				if (body && body.teamComponent) {
					body.teamComponent._teamIndex = oldIndex;
				}
			}
		}

		private static int OnCountingPlayersInRadius(On.RoR2.HoldoutZoneController.orig_CountPlayersInRadius originalMethod, HoldoutZoneController @this, UnityEngine.Vector3 origin, float chargingRadiusSqr, TeamIndex teamIndex) {
			if (teamIndex == TeamIndex.Void) {
				return originalMethod(@this, origin, chargingRadiusSqr, teamIndex);
			}
			int originalRetn = originalMethod(@this, origin, chargingRadiusSqr, teamIndex);
			return originalRetn + originalMethod(@this, origin, chargingRadiusSqr, TeamIndex.Void);
		}

		private static int OnCountingLivingPlayers(On.RoR2.HoldoutZoneController.orig_CountLivingPlayers originalMethod, TeamIndex teamIndex) {
			if (teamIndex == TeamIndex.Void) return originalMethod(teamIndex);
			int originalRetn = originalMethod(teamIndex);
			return originalRetn + originalMethod(TeamIndex.Void);
		}

		#endregion

		/// <summary>
		/// Behavior for the Newly Hatched Zoea spawn skin override.
		/// </summary>
		public enum ZoeaSkinBehavior {
			
			/// <summary>
			/// All void allies that are spawned will use the light blue ally skins.
			/// </summary>
			AlwaysAllySkin,

			/// <summary>
			/// Void allies spawn using their native dark skin iff the player with the item is a void survivor.
			/// </summary>
			IfOwnerIsVoid,

			/// <summary>
			/// Void allies always spawn using their native dark skin.
			/// </summary>
			AlwaysNativeSkin

		}
	}
}
