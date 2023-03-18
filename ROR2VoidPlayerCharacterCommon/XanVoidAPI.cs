using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior;
using Xan.ROR2VoidPlayerCharacterCommon.Registration;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

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
			if (damage.HasModdedDamageType(VoidDamageTypes.DisplayVoidDeathOnKill)) {
				if (isBlacklisted) {
					Log.LogError("Malformed damage type; someone has flagged the damage type as one to cause the void death effect, but also gave it the type telling it to *not* do this! Make up your mind!");
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
			// VoidDamageHooks.RegisterForManualVoidDeath(registrar, bodyIndex);
		}

		/// <summary>
		/// Call this <strong>AFTER</strong> registering your survivor. The input body should be its prefab. This will output information about the character into the console, making sure everything is set up properly.
		/// </summary>
		/// <param name="body"></param>
		public static bool VerifyProperConstruction(CharacterBody body) {
			bool a = VoidBehaviorRegistry.VerifyProperConstruction(body);
			// bool b = VoidDamageHooks.VerifyProperConstruction(body);
			return a;
		}

		/// <summary>
		/// Register the provided <see cref="BodyIndex"/> with that mod's associated config options. These config options can be used to override the global defaults for void damage behavior.
		/// <para/>
		/// If you don't want to make these config options yourself, see <see cref="CreateAndRegisterBlackHoleBehaviorConfigs(BaseUnityPlugin, AdvancedConfigBuilder, BodyIndex)"/>
		/// </summary>
		/// <param name="registrar"></param>
		/// <param name="bodyIndex"></param>
		/// <param name="useModSettings"></param>
		/// <param name="allowInstakillMonsters"></param>
		/// <param name="allowInstakillBosses"></param>
		/// <param name="allowFriendlyFire"></param>
		/// <param name="fallbackDamage"></param>
		public static void RegisterBlackHoleBehaviorOverrides(BaseUnityPlugin registrar, BodyIndex bodyIndex, ConfigEntry<bool> useModSettings, ReplicatedConfigEntry<bool> allowInstakillMonsters, ReplicatedConfigEntry<bool> allowInstakillBosses, ReplicatedConfigEntry<bool> allowFriendlyFire, ReplicatedConfigEntry<float> fallbackDamage) {
			CustomVoidDamageBehaviors.RegisterConfigProxy(registrar, bodyIndex, useModSettings, allowInstakillMonsters, allowInstakillBosses, allowFriendlyFire, fallbackDamage);
		}

		/// <summary>
		/// This can be called by implementors to automatically add the settings for controlling the black hole.
		/// </summary>
		/// <param name="registrar"></param>
		/// <param name="aCfg"></param>
		/// <param name="bodyIndex"></param>
		public static void CreateAndRegisterBlackHoleBehaviorConfigs(BaseUnityPlugin registrar, AdvancedConfigBuilder aCfg, BodyIndex bodyIndex) {
			aCfg.SetCategory("Void Common API");
			const string blackHoleDisclaimer = "<style=cIsVoid>This applies strictly only to player characters that have void deaths. This does not affect AI in any way.</style>\n\n";
			ConfigEntry<bool> useModSettings = aCfg.BindLocal("Use Mod Settings", "If true, the settings in this category will be used. If false, the equivalent settings in Void Common API's Global settings will be used instead.", false);
			ReplicatedConfigEntry<bool> allowInstakillMonsters = aCfg.BindReplicated("Instakill Monsters", $"{blackHoleDisclaimer}If true, black holes will instantly kill all <style=cIsDamage>monsters</style> via void death. If false, the fallback damage will apply.", true);
			ReplicatedConfigEntry<bool> allowInstakillBosses = aCfg.BindReplicated("Instakill Bosses", $"{blackHoleDisclaimer}If true, black holes will instantly kill all <style=cIsDamage>bosses</style> via void death. If false, the fallback damage will apply.", false);
			ReplicatedConfigEntry<bool> allowFriendlyFire = aCfg.BindReplicated("Friendly Fire", $"{blackHoleDisclaimer}If true, black holes can also kill members of the same team (such as player to player kills).", true);
			ReplicatedConfigEntry<float> fallbackDamage = aCfg.BindReplicated("Fallback Damage", $"{blackHoleDisclaimer}This value, as a multiplier (1 is 1x, 2 is 2x, ...), is applied to the player's base damage if their black hole cannot kill an enemy, boss or otherwise. It is recommended to make this value relatively high.", 100f, 0f, 1000f, 1, formatString: "{0}x");
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
					result.Append(" <style=cIsDamage>monsters</style>");
					and = true;
				}
				if (!bosses) {
					if (and) result.Append(" and");
					result.Append(" <style=cIsDamage>bosses</style>");
				}
				result.Append(" caught within its radius.");
			}

			return result.ToString();
		}



		/// <summary>
		/// Overrides a language token if it's present already. This is internal because it bypasses what LanguageAPI was made for, but that's okay if it's used sparingly.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="value"></param>
		public static void AddOrReplaceLang(string token, string value) {
			if (_languageApiCustomLanguage == null) {
				FieldInfo customLangFld = typeof(LanguageAPI).GetField("CustomLanguage", BindingFlags.NonPublic | BindingFlags.Static);
				_languageApiCustomLanguage = (Dictionary<string, Dictionary<string, string>>)customLangFld.GetValue(null);
			}
			_genericLang ??= _languageApiCustomLanguage["generic"];
			_genericLang[token] = value;
		}

		private static Dictionary<string, Dictionary<string, string>> _languageApiCustomLanguage = null;
		private static Dictionary<string, string> _genericLang = null;
	}
}
