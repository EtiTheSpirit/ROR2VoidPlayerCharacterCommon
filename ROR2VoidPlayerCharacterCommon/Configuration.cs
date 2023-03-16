using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon {

	/// <summary>
	/// All customizations for this mod are here.
	/// </summary>
	internal static class Configuration {

		/// <summary>
		/// If true, <see cref="Log.LogTrace(object)"/> will function. If false, it will not. Trace logging is extremely detailed but is often not necessary outside of debugging.
		/// </summary>
		public static bool TraceLogging => _traceLogging.Value;

		/// <summary>
		/// In some cases, like Simulacrum, void(touched) enemies can take damage from the fog when they should otherwise be immune. This fixes the problem by automatically registering all void characters
		/// for fog immunity regardless of which team they are on. This also includes friendly void units.
		/// </summary>
		public static bool EnforceVoidNativeImmunity => _enforceNativeImmunity.Value;

		/// <summary>
		/// If true, the low pass filter on death will be turned off iff the player had a void death animation.
		/// </summary>
		public static bool DisableMufflerOnDeath => _disableMufflerOnDeath.Value;

		/// <summary>
		/// If true, the black hole attack can instakill just like normal ones can. <strong>This only applies to players.</strong>
		/// </summary>
		public static bool AllowPlayerBlackHoleToInstakill => _allowPlayerBlackholeInstakill.Value;

		/// <summary>
		/// If true, the black hole can also instakill bosses, with the exception of Mithrix and Voidling who are both immune to VoidDeath.
		/// </summary>
		public static bool AllowPlayerBlackHoleToInstakillBosses => _allowPlayerBlackholeInstakillBosses.Value;

		/// <summary>
		/// If true, black holes spawned by players can, much like that of friendly void units, kill other players.
		/// </summary>
		public static bool BlackHoleFriendlyFire => _allowPlayerBlackholeFriendlyFire.Value;

		/// <summary>
		/// If a black hole is unable to kill an enemy (see <see cref="AllowPlayerBlackHoleToInstakill"/> and <see cref="AllowPlayerBlackHoleToInstakillBosses"/>), it will do this much % base damage instead.
		/// </summary>
		public static float BlackHoleBackupDamage => _blackholeBackupDamage.Value;

		#region Backing Fields

		[AllowNull]
		private static ConfigEntry<bool> _traceLogging;

		[AllowNull]
		private static ConfigEntry<bool> _enforceNativeImmunity;

		[AllowNull]
		private static ConfigEntry<bool> _disableMufflerOnDeath;

		[AllowNull]
		internal static ConfigEntry<bool> _allowPlayerBlackholeInstakill;

		[AllowNull]
		internal static ConfigEntry<bool> _allowPlayerBlackholeInstakillBosses;

		[AllowNull]
		internal static ConfigEntry<bool> _allowPlayerBlackholeFriendlyFire;

		[AllowNull]
		internal static ConfigEntry<float> _blackholeBackupDamage;

		#endregion

		internal static void Initialize(ConfigFile cfg) {
			Log.LogMessage("Initializing Configuration...");
			AdvancedConfigBuilder aCfg = new AdvancedConfigBuilder(cfg, null, VoidPlayerCharacterCommon.PLUGIN_GUID, "Void Common API", "The Void Common API manages all Void-related quirks and mechanics that are implemented by my Void-related mods, such as the playable characters. The settings here are relatively minimal, only declaring some gameplay tweaks and the global defaults for options that are a part of each individual Void character.");
			aCfg.SetCategory("Mod Meta");
			_traceLogging = aCfg.Bind("Trace Logging", false, "Trace Logging is a form of precise logging that gives status updates on what's happening in the code very granularly.\n\nWhile good for debugging, these can dramatically increase log file size. Consider only using it when reporting bugs.");

			aCfg.SetCategory("Void Entity Behavior");
			_enforceNativeImmunity = aCfg.Bind("Enforce Void Immunity", false, "If enabled, all void(touched) enemies and allies will be immune to damage from the void fog and atmosphere.\n\nThis resolves a few weird edge cases where void enemies can take damage from the atmosphere, such as in The Simulacrum.");

			aCfg.SetCategory("Player Preferences");
			_disableMufflerOnDeath = aCfg.Bind("Disable Low Pass Filter", true, "If enabled, and if the current player has a void death animation, this will disable the low pass filter upon death so that the sound effects of the black hole are not affected.");

			aCfg.SetCategory("Global Black Hole Behavior");
			const string blackHoleDisclaimer = "<style=cIsVoid><style=cIsHealth>Host only.</style> This applies strictly only to player characters that have void deaths. This does not affect AI in any way. <style=cIsUtility>These settings can be configured per individual Void characters. These are the global defaults.</style></style>\n\n";

			_allowPlayerBlackholeInstakill = aCfg.Bind("Instakill Monsters", true, $"{blackHoleDisclaimer}If true, black holes will instantly kill all <style=cIsDamage>monsters</style> via void death. If false, the fallback damage will apply.");
			_allowPlayerBlackholeInstakillBosses = aCfg.Bind("Instakill Bosses", false, $"{blackHoleDisclaimer}If true, black holes will instantly kill all <style=cIsDamage>bosses</style> via void death. If false, the fallback damage will apply.");
			_allowPlayerBlackholeFriendlyFire = aCfg.Bind("Friendly Fire", true, $"{blackHoleDisclaimer}If true, black holes can also kill members of the same team (such as player to player kills).");
			_blackholeBackupDamage = aCfg.Bind("Fallback Damage", 250f, $"{blackHoleDisclaimer}This value, as a multiplier (1 is 1x, 2 is 2x, ...), is applied to the player's base damage if their black hole cannot kill an enemy, boss or otherwise. It is recommended to make this value relatively high.", 0f, 1000f, 1, formatString: "{0}x");
		}
	}
}