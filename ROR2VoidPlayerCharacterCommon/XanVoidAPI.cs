﻿using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior;
using Xan.ROR2VoidPlayerCharacterCommon.Registration;

namespace Xan.ROR2VoidPlayerCharacterCommon {

	/// <summary>
	/// All API members for you to use in the code that involve getting and registering information reside here.
	/// </summary>
	public static class XanVoidAPI {

		/// <summary>
		/// Returns whether or not the provided <see cref="BodyIndex"/> has been registered as one that is immune to void fog.
		/// </summary>
		/// <param name="bodyIndex"></param>
		/// <returns></returns>
		public static bool IsImmuneToVoidFog(BodyIndex bodyIndex) => VoidBehaviorRegistry.IsImmuneToVoidFog(bodyIndex);

		/// <summary>
		/// Returns true if the provided <see cref="DamageInfo"/> should cause the void death visual effect.
		/// </summary>
		/// <param name="damage"></param>
		/// <returns></returns>
		public static bool ShouldShowVoidDeath(DamageInfo damage) {
			bool isBlacklisted = damage.HasModdedDamageType(VoidDamageTypes.BlacklistExaggeratedVoidDeath);
			if (damage.damageType.HasFlag(DamageType.VoidDeath)) return !isBlacklisted;
			if (damage.HasModdedDamageType(VoidDamageTypes.DisplayVoidDeathOnDeath)) {
				if (isBlacklisted) {
					Log.LogError("Malformed damage type; someone has flagged the damage type as one to cause the Lost Seer's Lenses effect, but also gave it the type telling it to *not* do this! Make up your mind!");
					Log.LogError((new StackTrace()).ToString());
					Log.LogWarning($"Attacker: {damage.attacker}");
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// This method checks if a black hole spawned by a player can instakill normal monsters.
		/// </summary>
		/// <param name="bodyIndex"></param>
		/// <returns></returns>
		public static bool CanCharacterInstakillMonsters(BodyIndex bodyIndex) => CustomVoidDamageBehaviors.CanCharacterInstakillMonsters(bodyIndex);

		/// <summary>
		/// This method checks if a black hole spawned by a player can instakill normal bosses, with the exception of Mithrix and Voidling, who are both immune to this attack.
		/// </summary>
		/// <param name="bodyIndex"></param>
		/// <returns></returns>
		public static bool CanCharacterInstakillBosses(BodyIndex bodyIndex) => CustomVoidDamageBehaviors.CanCharacterInstakillBosses(bodyIndex);

		/// <summary>
		/// This method checks if a black hole spawned by a player can kill other players.
		/// </summary>
		/// <param name="bodyIndex"></param>
		/// <returns></returns>
		public static bool CanCharacterFriendlyFire(BodyIndex bodyIndex) => CustomVoidDamageBehaviors.CanCharacterFriendlyFire(bodyIndex);

		/// <summary>
		/// If a black hole cannot instakill something, this is the % base damage that it should do instead.
		/// </summary>
		/// <param name="bodyIndex"></param>
		/// <returns></returns>
		public static float GetFallbackDamage(BodyIndex bodyIndex) => CustomVoidDamageBehaviors.GetFallbackDamage(bodyIndex);

		/// <summary>
		/// Register this character to be immune to Void Fog. This is both the visual effect and the damage at once. This <strong>MUST</strong> be called after the body catalog is available! See <see cref="BodyCatalog.availability"/> for more information (consider using its event).
		/// </summary>
		/// <param name="registrar">Your mod's plugin class. This is used to keep track of who did what.</param>
		/// <param name="bodyIndex">The <see cref="CharacterBody"/> to register, by its index.</param>
		public static void RegisterAsVoidEntity(BaseUnityPlugin registrar, BodyIndex bodyIndex) {
			VoidBehaviorRegistry.RegisterForVoidImmunities(registrar, bodyIndex);
			VoidDamageHooks.RegisterForManualVoidDeath(registrar, bodyIndex);
		}

		/// <summary>
		/// Call this <strong>AFTER</strong> registering your survivor. The input body should be its prefab. This will output information about the character into the console, making sure everything is set up properly.
		/// </summary>
		/// <param name="body"></param>
		public static bool VerifyProperConstruction(CharacterBody body) {
			bool a = VoidBehaviorRegistry.VerifyProperConstruction(body);
			bool b = VoidDamageHooks.VerifyProperConstruction(body);
			return a && b;
		}

		/// <summary>
		/// Register the provided <see cref="BodyIndex"/> with that mod's associated config options. These config options can be used to override the global defaults for void damage behavior.
		/// <para/>
		/// If you don't want to make these config options yourself, see <see cref="CreateAndRegisterBlackHoleBehaviorConfigs(BaseUnityPlugin, ConfigFile, BodyIndex)"/>
		/// </summary>
		/// <param name="registrar"></param>
		/// <param name="bodyIndex"></param>
		/// <param name="useModSettings"></param>
		/// <param name="allowInstakillMonsters"></param>
		/// <param name="allowInstakillBosses"></param>
		/// <param name="allowFriendlyFire"></param>
		/// <param name="fallbackDamage"></param>
		public static void RegisterBlackHoleBehaviorOverrides(BaseUnityPlugin registrar, BodyIndex bodyIndex, ConfigEntry<bool> useModSettings, ConfigEntry<bool> allowInstakillMonsters, ConfigEntry<bool> allowInstakillBosses, ConfigEntry<bool> allowFriendlyFire, ConfigEntry<float> fallbackDamage) {
			CustomVoidDamageBehaviors.RegisterConfigProxy(registrar, bodyIndex, useModSettings, allowInstakillMonsters, allowInstakillBosses, allowFriendlyFire, fallbackDamage);
		}

		/// <summary>
		/// This can be called by implementors to automatically add the settings for controlling the black hole.
		/// </summary>
		/// <param name="registrar"></param>
		/// <param name="cfg"></param>
		/// <param name="bodyIndex"></param>
		public static void CreateAndRegisterBlackHoleBehaviorConfigs(BaseUnityPlugin registrar, ConfigFile cfg, BodyIndex bodyIndex) {
			ConfigEntry<bool> useModSettings = cfg.Bind("Void Character API", "Use Settings from This Mod", false, "If true, the settings in this category will be used to control the behavior of the black hole. If false, the global settings that come with the Void Character API will be used instead.");
			ConfigEntry<bool> allowInstakillMonsters = cfg.Bind("Void Character API", "Black Holes Instakill Monsters", true, "You probably want this to be true. This setting allows black holes from void deaths to instakill monsters.");
			ConfigEntry<bool> allowInstakillBosses = cfg.Bind("Void Character API", "Black Holes Instakill Bosses", false, "You probably want this to be false. This setting allows black holes from void deaths to instakill bosses, with the exception of Mithrix and Voidling who are immune to this type of damage.");
			ConfigEntry<bool> allowFriendlyFire = cfg.Bind("Void Character API", "Black Hole Friendly Fire", true, "If true, black holes spawned by friendly void players can, much like those of friendly void NPCs, kill players.");
			ConfigEntry<float> fallbackDamage = cfg.Bind("Void Character API", "Black Hole Fallback Damage", 750f, "This value, as a multiplier (1 is 1x, 2 is 2x, ...), is applied to the player's base damage if their black hole cannot kill an enemy, boss or otherwise.");
			RegisterBlackHoleBehaviorOverrides(registrar, bodyIndex, useModSettings, allowInstakillMonsters, allowInstakillBosses, allowFriendlyFire, fallbackDamage);
		}

		/// <summary>
		/// For localization, this uses the config proxy system (see <see cref="RegisterBlackHoleBehaviorOverrides"/>). This builds a string that describes what a black hole does to various targets.
		/// </summary>
		/// <returns></returns>
		public static string BuildBlackHoleDescription(BodyIndex bodyIndex, bool lowercaseStart = false) {
			bool monsters = CanCharacterInstakillMonsters(bodyIndex);
			bool bosses = CanCharacterInstakillBosses(bodyIndex);
			bool players = CanCharacterFriendlyFire(bodyIndex);
			float fallback = GetFallbackDamage(bodyIndex);

			StringBuilder result = new StringBuilder();
			result.Append(lowercaseStart ? 't' : 'T');
			result.Append("ear a rift into the void that, after a short delay, ");
			int listSize = 0;
			if (monsters || bosses || players) {
				result.Append("<style=cDeath>instantly kills</style> all");

				if (monsters) {
					result.Append(" <style=cIsDamage>monsters</style>");
					listSize++;
				}
				if (bosses) {
					if (listSize > 0) result.Append(',');
					result.Append(" <style=cIsDamage>bosses</style>");
					listSize++;
				}
				if (players) {
					if (listSize == 0) {
						result.Append(" players");
					} else if (listSize == 1) {
						result.Append(" and <style=cIsDamage>players</style>");
					} else {
						result.Append(", and <style=cIsDamage>players</style>");
					}
					listSize++;
				}
				result.Append(" caught within its radius. ");
			}
			if (!monsters || !bosses) {
				if (listSize > 0) {
					result.Append("It instead ");
				}
				result.Append($"deals <style=cIsDamage>{((int)Math.Floor(fallback)) * 100}% base damage</style> to all");
				bool and = false;
				if (!monsters) {
					result.Append(" monsters");
					and = true;
				}
				if (!bosses) {
					if (and) result.Append(" and");
					result.Append(" bosses");
				}
				result.Append(" caught within its radius.");
			}

			return result.ToString();
		}
	}
}