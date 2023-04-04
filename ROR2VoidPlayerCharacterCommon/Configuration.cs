using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;
using static Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper.VoidTeamSurvivorController;

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
		/// Puts players using the Void survivors onto the Void team.
		/// </summary>
		public static bool VoidTeamPlayers => _putVoidPlayersOnVoidTeam.Value;

		/// <summary>
		/// The type of skin that friendly Void enemies spawned via Newly Hatched Zoea will use.
		/// </summary>
		public static ZoeaSkinBehavior ZoeaSkinType => _useNativeZoeaSkinOnVoid.Value;

		/// <summary>
		/// The damage factor in PvP specifically in the direction of Player => Void Player (and not the other way around).
		/// </summary>
		public static float PvPToVoidTeamDamageMult => _pvpVoidDamage.Value;

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

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<bool> _putVoidPlayersOnVoidTeam;

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<ZoeaSkinBehavior> _useNativeZoeaSkinOnVoid;

		[ReplicatedConfiguration]
		internal static ReplicatedConfigEntry<float> _blackholeBackupDamage;

		[ReplicatedConfiguration]
		internal static ReplicatedPercentageWrapper _pvpVoidDamage;

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
			_putVoidPlayersOnVoidTeam = aCfg.BindReplicated("Void Players on Void Team", "<style=cDeath>This is experimental, and was mostly added for the sake of playing around. This is known to cause issues. Handle with care.</style>\n\nPlayers that spawn in as registered Void survivors (not including Void Fiend) will be put onto the Void team. Which means their friends (you) can kill them. And they can kill you. <style=cDeath>This introduces a significant number of weird behaviors that dramatically alter the run</style>. Alongside treating players like enemies (because, I mean, they <i>are</i>...), certain features do not work such as logbook pickups. If you are in singleplayer, entering the Deep Void will softlock your game because you are allies with the Voidling.\n\n<style=cIsDamage>Aurelionite</style> has had older code reinstated. When counting how many Halcyon Seeds that every player has, it also keeps track of the team that has the most seeds in total. This means if you are in multiplayer and your friendly Void creature(s) picks up more seeds in total than the normal players did, Aurelionite will be on the Void team.", false, AdvancedConfigBuilder.RestartType.NextRespawn);
			_pvpVoidDamage = aCfg.BindFloatPercentageReplicated("Void Player PvP Damage", "<style=cDeath>This is experimental, and was mostly added for the sake of playing around. This might break things. Handle with care.</style>\n\nDamage dealt between Players and Void Players is reduced to this percentage of its original amount.", 10f);
			_useNativeZoeaSkinOnVoid = aCfg.BindReplicated("Native Zoea", "If a survivor has the Newly Hatched Zoea, the spawned Void allies can be set up to use their native skins instead of the custom ally appearance. This effect is purely cosmetic. It may be a good time to remind you that the reason the alt skins were added was because it was hard to tell friendlies from enemies.", ZoeaSkinBehavior.AlwaysAllySkin, AdvancedConfigBuilder.RestartType.NextRespawn);

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