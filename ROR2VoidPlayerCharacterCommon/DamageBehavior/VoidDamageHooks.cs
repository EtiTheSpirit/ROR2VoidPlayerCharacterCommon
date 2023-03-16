#pragma warning disable Publicizer001
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
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

		private static readonly Dictionary<BodyIndex, BaseUnityPlugin> _manualVoidDeath = new Dictionary<BodyIndex, BaseUnityPlugin>();

		internal static void Initialize() {
			Log.LogMessage("Initializing Void Damage and Effect Hooks...");
			On.RoR2.HealthComponent.TakeDamage += InterceptTakeDamageForVoidResist;
			On.RoR2.HealthComponent.TakeDamage += InterceptTakeDamageForConditionalVoid;
			On.RoR2.HealthComponent.TakeDamage += InterceptTakeDamageForVoidDeathOnKill;
			On.RoR2.CharacterBody.SetBuffCount += InterceptBuffsEventForVoidResist;
			On.RoR2.CharacterMaster.OnBodyDeath += PreventGameOverOnDeath;
			On.RoR2.MusicController.RecalculateHealth += OnRecalculateHealthForLPF;
			On.RoR2.Projectile.ProjectileController.DispatchOnInitialized += InterceptProjectileForFriendlyFire;
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


		/// <summary>
		/// Prevents the game over screen from showing momentarily, based on the death state type of the character that died.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		/// <param name="body"></param>
		private static void PreventGameOverOnDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath originalMethod, CharacterMaster @this, CharacterBody body) {
			originalMethod(@this, body);
			if (body) {
				CharacterDeathBehavior deathBehavior = body.GetComponent<CharacterDeathBehavior>();
				if (deathBehavior) {
					Type stateType = deathBehavior.deathState.stateType;
					if (stateType != null) {
						if (stateType.GetInterface(nameof(IHasDelayedGameOver)) != null) {
							@this.preventGameOver = true;
						}
					}
				}
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
		/// This method ties into <see cref="VoidDamageTypes.ConditionalVoidDeath"/>. If damage won't kill an enemy, it should apply like normal damage, however if the damage *will* kill an enemy
		/// then it needs to be flagged as <see cref="DamageType.VoidDeath"/> iff that enemy is not immune to it, so that the proper VFX displays.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		/// <param name="damageInfo"></param>
		/// <exception cref="NotImplementedException"></exception>
		private static void InterceptTakeDamageForVoidDeathOnKill(On.RoR2.HealthComponent.orig_TakeDamage originalMethod, HealthComponent @this, DamageInfo damageInfo) {
			bool skip =																			// Skip if...
				   !@this.body																	// ...the body is missing, or
				|| damageInfo.rejected															// ...the damage has already been rejected by something else, or
				|| damageInfo.damageType.HasFlag(DamageType.VoidDeath)							// ...the damage is already classified as void death, or
				|| @this.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToVoidDeath)		// ...the receiving body is immune to void death, or
				|| !damageInfo.HasModdedDamageType(VoidDamageTypes.DisplayVoidDeathOnKill);		// ...the damage is not registered to cause the vfx.


			if (skip) {
				originalMethod(@this, damageInfo);
				return;
			}

			bool wasAlive = @this.alive;
			originalMethod(@this, damageInfo);
			if (damageInfo.rejected) return; // Do nothing if someone rejected the damage from occurring.

			if (wasAlive && !@this.alive) @this.killingDamageType = DamageType.VoidDeath; // Modify this.
		}


		#region Void Fog Resistance

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

			if (@this.body != null && VoidBehaviorRegistry.IsImmuneToVoidFog(@this.body.bodyIndex) && !damageInfo.HasModdedDamageType(VoidDamageTypes.BypassFogResistance)) {
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

		#endregion

		#region Conditional Void Death

		/// <summary>
		/// Executes before <see cref="HealthComponent.TakeDamage(DamageInfo)"/> and conditionally applies or removes the <see cref="DamageType.VoidDeath"/> type based on the settings of the damage.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		/// <param name="damageInfo"></param>
		private static void InterceptTakeDamageForConditionalVoid(On.RoR2.HealthComponent.orig_TakeDamage originalMethod, HealthComponent @this, DamageInfo damageInfo) {
			if (damageInfo.rejected) {
				originalMethod(@this, damageInfo);
				return;
			}

			if (damageInfo.HasModdedDamageType(VoidDamageTypes.ConditionalVoidDeath)) {
				GameObject attacker = damageInfo.attacker;
				if (attacker != null) {
					CharacterBody body = attacker.GetComponent<CharacterBody>();
					if (body != null) {
						Log.LogTrace("Detected TakeDamage call with Conditional Void Death!");
						// New condition 
						bool canInstakill;
						CharacterBody.BodyFlags theseFlags = @this.body.bodyFlags;
						if (theseFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToVoidDeath) || damageInfo.attacker == @this.gameObject) {
							canInstakill = false;
						} else {
							bool isBoss = @this.body.isBoss;
							TeamComponent victimTeam = @this.GetComponent<TeamComponent>();
							TeamComponent attackerTeam = body.GetComponent<TeamComponent>();
							if (victimTeam && attackerTeam && victimTeam.teamIndex == attackerTeam.teamIndex) {
								canInstakill = CustomVoidDamageBehaviors.CanCharacterFriendlyFire(body.bodyIndex);
							} else {
								if (isBoss) {
									canInstakill = CustomVoidDamageBehaviors.CanCharacterInstakillBosses(body.bodyIndex);
								} else {
									canInstakill = CustomVoidDamageBehaviors.CanCharacterInstakillMonsters(body.bodyIndex);
								}
							}
						}

						if (canInstakill) {
							Log.LogTrace($"Intercepted damage for Conditional Void Death. Instakill has been approved, and will occur if possible.");
							damageInfo.damageType |= DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection | DamageType.VoidDeath;
						} else {
							Log.LogTrace($"Intercepted damage for Conditional Void Death. Instakill has been rejected, and has been removed. Applying fallback damage instead.");
							damageInfo.damageType &= ~DamageType.VoidDeath;
							damageInfo.damage = body.baseDamage * CustomVoidDamageBehaviors.GetFallbackDamage(body.bodyIndex);
						}
					}
				}
			}
			originalMethod(@this, damageInfo);
		}

		/// <summary>
		/// Executes before dispatching a projectile, modifying its damage type.
		/// </summary>
		/// <param name="originalMethod"></param>
		/// <param name="this"></param>
		private static void InterceptProjectileForFriendlyFire(On.RoR2.Projectile.ProjectileController.orig_DispatchOnInitialized originalMethod, ProjectileController @this) {
			DamageAPI.ModdedDamageTypeHolderComponent modDmg = @this.gameObject.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
			if (modDmg && modDmg.Has(VoidDamageTypes.ConditionalVoidDeath)) {
				if (@this.owner) {
					CharacterBody body = @this.owner.GetComponent<CharacterBody>();
					if (body && CustomVoidDamageBehaviors.CanCharacterFriendlyFire(body.bodyIndex)) {
						Log.LogTrace("A projectile with Conditional Void Death was created and its creator can friendly fire. Setting its team to Neutral.");
						TeamFilter filter = @this.gameObject.GetComponent<TeamFilter>();
						if (!filter) filter = @this.gameObject.AddComponent<TeamFilter>();
						filter.teamIndex = TeamIndex.Neutral;
						@this.teamFilter = filter;
					}
				}
			}
			originalMethod(@this);
		}

		#endregion

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
