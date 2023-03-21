using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
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

		private static ConfigEntry<bool> _traceLogging;

		private static ConfigEntry<bool> _disableMufflerOnDeath;

		[ReplicatedConfiguration]
		private static ReplicatedConfigEntry<bool> _enforceNativeImmunity;

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<bool> _allowPlayerBlackholeInstakill;

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<bool> _allowPlayerBlackholeInstakillBosses;

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<bool> _allowPlayerBlackholeFriendlyFire;

		//[ReplicatedConfiguration]
		//internal static ReplicatedConfigEntry<bool> _putVoidPlayersOnVoidTeam;

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<float> _blackholeBackupDamage;

		#endregion

		internal static void Initialize(ConfigFile cfg) {
			Log.LogMessage("Initializing Configuration...");
			AdvancedConfigBuilder aCfg = new AdvancedConfigBuilder(typeof(Configuration), cfg, null, VoidPlayerCharacterCommon.PLUGIN_GUID, "Void Common API", "The Void Common API manages all Void-related quirks and mechanics that are implemented by my Void-related mods, such as the playable characters. The settings here are relatively minimal, only declaring some gameplay tweaks and the global defaults for options that are a part of each individual Void character.");
			aCfg.SetCategory("Mod Meta");
			_traceLogging = aCfg.BindLocal("Trace Logging", "Trace Logging is a form of precise logging that gives status updates on what's happening in the code very granularly.\n\nWhile good for debugging, these can dramatically increase log file size. Consider only using it when reporting bugs.", false);

			aCfg.SetCategory("Player Preferences");
			_disableMufflerOnDeath = aCfg.BindLocal("Disable Low Pass Filter", "If enabled, and if the current player has a void death animation, this will disable the low pass filter upon death so that the sound effects of the black hole are not affected.", true);

			aCfg.SetCategory("Void Entity Behavior");
			_enforceNativeImmunity = aCfg.BindReplicated("Native Void Fog Immunity", "If enabled, all void(touched) enemies and allies will be immune to damage from the void fog and atmosphere.\n\nThis resolves a few weird edge cases where void enemies can take damage from the atmosphere, such as in The Simulacrum. Handle this setting with care.", false);
			// _putVoidPlayersOnVoidTeam = aCfg.BindReplicated("Void Players on Void Team", "This is probably a terrible idea and honestly I added this just because it seemed silly at the time. Players that spawn in as registered Void survivors (not including Void Fiend) will be put onto the Void team. Which means their friends (you) can kill them.", false, AdvancedConfigBuilder.RestartType.NextRespawn);

			aCfg.SetCategory("Global Black Hole Behavior");
			const string blackHoleDisclaimer = "<style=cIsVoid>This applies strictly only to player characters that have void deaths. This does not affect AI in any way.</style> Also note that these settings can be configured per individual Void characters (these are just the global defaults).\n\n";

			_allowPlayerBlackholeInstakill = aCfg.BindReplicated("Instakill Monsters", $"{blackHoleDisclaimer}If true, black holes will instantly kill all <style=cIsDamage>monsters</style> via void death. If false, the fallback damage will apply.", true);
			_allowPlayerBlackholeInstakillBosses = aCfg.BindReplicated("Instakill Bosses", $"{blackHoleDisclaimer}If true, black holes will instantly kill all <style=cIsDamage>bosses</style> via void death. If false, the fallback damage will apply.", false);
			_allowPlayerBlackholeFriendlyFire = aCfg.BindReplicated("Friendly Fire", $"{blackHoleDisclaimer}If true, black holes can also kill members of the same team (such as player to player kills).", true);
			_blackholeBackupDamage = aCfg.BindReplicated("Fallback Damage", $"{blackHoleDisclaimer}This value, as a multiplier (1 is 1x, 2 is 2x, ...), is applied to the player's base damage if their black hole cannot kill an enemy, boss or otherwise. It is recommended to make this value relatively high.", 100f, 0f, 1000f, 1, formatString: "{0}x");

			aCfg.CreateConfigAutoReplicator();
		}
	}
}