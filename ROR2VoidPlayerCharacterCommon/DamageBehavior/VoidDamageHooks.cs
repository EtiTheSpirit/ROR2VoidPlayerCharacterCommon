using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.EntityStates;
using Xan.ROR2VoidPlayerCharacterCommon.Registration;

namespace Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior {

	/// <summary>
	/// All IL hooks are managed here. This is also where useful references to the Void Fog effects are managed.
	/// </summary>
	public static class VoidDamageHooks {

		internal static void Initialize() {
			Log.LogMessage("Initializing Void Damage and Effect Hooks...");
			On.RoR2.HealthComponent.TakeDamage += InterceptTakeDamageForVoidResist;
			On.RoR2.CharacterBody.SetBuffCount += InterceptBuffsEventForVoidResist;
			On.RoR2.CharacterMaster.OnBodyDeath += PreventGameOverOnDeath;
			On.RoR2.MusicController.RecalculateHealth += OnRecalculateHealthForLPF;
			BodyCatalog.availability.onAvailable += OnCharacterBodyRegistrationComplete;
		}

		private static void OnCharacterBodyRegistrationComplete() {
			if (Configuration.EnforceVoidNativeImmunity) {
				Log.LogTrace("Making all void enemies immune to the fog (they do not care that the fog is coming)...");
				BodyIndex infestor = BodyCatalog.FindBodyIndex("VoidInfestorBody");
				BodyIndex barnacle = BodyCatalog.FindBodyIndex("VoidBarnacleBody");
				BodyIndex reaver = BodyCatalog.FindBodyIndex("NullifierBody");
				BodyIndex jailer = BodyCatalog.FindBodyIndex("VoidJailerBody");
				BodyIndex devastator = BodyCatalog.FindBodyIndex("VoidMegaCrabBody");
				BodyIndex reaverAlly = BodyCatalog.FindBodyIndex("NullifierAllyBody");
				BodyIndex jailerAlly = BodyCatalog.FindBodyIndex("VoidJailerAllyBody");
				BodyIndex devastatorAlly = BodyCatalog.FindBodyIndex("VoidMegaCrabAllyBody");

				Log.LogTrace($"Indices: Void Infestor={infestor}, Void Barnacle={barnacle}, Void Reaver={reaver} (Friendly={reaverAlly}), Void Jailer={jailer} (Friendly={jailerAlly}), Void Devastator={devastator} (Friendly={devastatorAlly})");
				if (infestor != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, infestor);
				if (barnacle != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, barnacle);
				if (reaver != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, reaver);
				if (jailer != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, jailer);
				if (devastator != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, devastator);
				if (reaverAlly != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, reaverAlly);
				if (jailerAlly != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, jailerAlly);
				if (devastatorAlly != BodyIndex.None) VoidBehaviorRegistry.RegisterForVoidImmunities(VoidPlayerCharacterCommon.Instance, devastatorAlly);
			}
		}


		private static void PreventGameOverOnDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath originalMethod, CharacterMaster @this, CharacterBody body) {
			originalMethod(@this, body);
			CharacterDeathBehavior deathBehavior = body.GetComponent<CharacterDeathBehavior>();
			if (deathBehavior.deathState.stateType.GetInterface(nameof(IHasDelayedGameOver)) != null) {
				@this.preventGameOver = true;
			}
		}


		/// <summary>
		/// When the character is dead, the muffle should not apply if they have a death animation.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		/// <param name="playerObject"></param>
		private static void OnRecalculateHealthForLPF(On.RoR2.MusicController.orig_RecalculateHealth originalMethod, MusicController @this, GameObject playerObject) {
			originalMethod(@this, playerObject);
			if (Configuration.DisableMufflerOnDeath) {
				if (@this.target) {
					CharacterBody body = @this.target.GetComponent<CharacterBody>();
					if (body) {
						HealthComponent healthComponent = body.healthComponent;
						if (healthComponent) {
							if (!healthComponent.alive) @this.rtpcPlayerHealthValue.value = 100f;
						}
					}
				}
			}
		}

		/// <summary>
		/// Intent: Do our best to see if damage from a Void Seed / Atmosphere is being dealt, and cancel it.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		/// <param name="damageInfo"></param>
		private static void InterceptTakeDamageForVoidResist(On.RoR2.HealthComponent.orig_TakeDamage originalMethod, HealthComponent @this, DamageInfo damageInfo) {
			if (damageInfo.rejected) {
				originalMethod(@this, damageInfo);
				return;
			}

			if (@this.body != null && VoidBehaviorRegistry.IsImmuneToVoidFog(@this.body.bodyIndex)) {
				if (damageInfo.attacker == null && damageInfo.inflictor == null && damageInfo.damageType == NO_BLOCK_NO_ARMOR) {
					Log.LogTrace("Rejecting damage for what I believe to be Void atmosphere damage (it has no source/attacker, and the damage type bypasses blocks and armor only).");
					damageInfo.rejected = true;
				}
			}

			originalMethod(@this, damageInfo);
		}

		/// <summary>
		/// Intent: Cancel void fog VFX.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		/// <param name="buffType"></param>
		/// <param name="newCount"></param>
		private static void InterceptBuffsEventForVoidResist(On.RoR2.CharacterBody.orig_SetBuffCount originalMethod, CharacterBody @this, BuffIndex buffType, int newCount) {
			if (VoidBehaviorRegistry.IsImmuneToVoidFog(@this.bodyIndex)) {
				if (buffType == MegaVoidFog || buffType == NormVoidFog || buffType == WeakVoidFog) {
					Log.LogTrace($"Rejecting attempt to add fog to {Helpers.BodyToString(@this)}'s status effects.");
					originalMethod(@this, buffType, 0); // Always 0
					return;
				}
			}

			originalMethod(@this, buffType, newCount);
		}

		#region Fog Types and Important Values

		/// <summary>
		/// This is the result of <see cref="DamageType.BypassBlock"/> | <see cref="DamageType.BypassArmor"/>.
		/// </summary>
		public const DamageType NO_BLOCK_NO_ARMOR = DamageType.BypassBlock | DamageType.BypassArmor;

		/// <summary>
		/// A reference to the highest power void fog, which is spawned by the Voidling.
		/// </summary>
		/// <value>
		/// Resolved upon referencing this property; points to <see cref="DLC1Content.Buffs.VoidRaidCrabWardWipeFog"/>.
		/// </value>
		public static BuffIndex MegaVoidFog {
			get {
				if (_megaVoidFog == BuffIndex.None) {
					_megaVoidFog = DLC1Content.Buffs.VoidRaidCrabWardWipeFog.buffIndex;
				}
				return _megaVoidFog;
			}
		}

		/// <summary>
		/// A reference to ordinary void fog. This is used in the Void Locus, Simulacrum, and Void Fields.
		/// </summary>
		/// <value>
		/// Resolved upon referencing this property; points to <see cref="RoR2Content.Buffs.VoidFogStrong"/>.
		/// </value>
		public static BuffIndex NormVoidFog {
			get {
				if (_normVoidFog == BuffIndex.None) {
					_normVoidFog = RoR2Content.Buffs.VoidFogStrong.buffIndex;
				}
				return _normVoidFog;
			}
		}

		/// <summary>
		/// A reference to weak void fog. I don't actually know when this is used chief I'm not gonna lie. I think it's void seeds.
		/// </summary>
		/// <value>
		/// Resolved upon referencing this property; points to <see cref="RoR2Content.Buffs.VoidFogMild"/>.
		/// </value>
		public static BuffIndex WeakVoidFog {
			get {
				if (_weakVoidFog == BuffIndex.None) {
					_weakVoidFog = RoR2Content.Buffs.VoidFogMild.buffIndex;
				}
				return _weakVoidFog;
			}
		}

		private static BuffIndex _megaVoidFog = BuffIndex.None;
		private static BuffIndex _normVoidFog = BuffIndex.None;
		private static BuffIndex _weakVoidFog = BuffIndex.None;

		#endregion

	}
}
