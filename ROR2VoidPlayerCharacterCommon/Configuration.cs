using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
		private static ConfigEntry<bool> _allowPlayerBlackholeInstakill;

		[AllowNull]
		private static ConfigEntry<bool> _allowPlayerBlackholeInstakillBosses;

		[AllowNull]
		private static ConfigEntry<bool> _allowPlayerBlackholeFriendlyFire;

		[AllowNull]
		private static ConfigEntry<float> _blackholeBackupDamage;

		#endregion

		internal static void Initialize(ConfigFile cfg) {
			Log.LogMessage("Initializing Configuration...");
			_traceLogging = cfg.Bind("Mod Meta", "Trace Logging", false, "Trace Logging is a form of precise logging that tracks every little thing that is being done. Most of this information is useless outside of debugging the mod. Consider enabling it when forming bug reports.");
			_enforceNativeImmunity = cfg.Bind("Void Behavior", "Enforce Immunity on Void Creatures", false, "If enabled, all void(touched) enemies and allies will be immune to damage from the void fog and atmosphere. This resolves a few weird edge cases where void enemies can take damage from the atmosphere.");
			_disableMufflerOnDeath = cfg.Bind("Void Behavior", "Disable Low Pass Filter on Death Animations", true, "If enabled, and if the current player has a void death animation, this will disable the low pass filter upon death so that the sound effects of the black hole are not affected.");
			_allowPlayerBlackholeInstakill = cfg.Bind("Global Black Hole Behavior", "Black Holes Instakill Monsters", true, "You probably want this to be true. This setting allows black holes from void deaths to instakill monsters.");
			_allowPlayerBlackholeInstakillBosses = cfg.Bind("Global Black Hole Behavior", "Black Holes Instakill Bosses", false, "You probably want this to be false. This setting allows black holes from void deaths to instakill bosses, with the exception of Mithrix and Voidling who are immune to this type of damage.");
			_allowPlayerBlackholeFriendlyFire = cfg.Bind("Global Black Hole Behavior", "Black Hole Friendly Fire", true, "If true, black holes spawned by friendly void players can, much like those of friendly void NPCs, kill players.");
			_blackholeBackupDamage = cfg.Bind("Global Black Hole Behavior", "Black Hole Fallback Damage", 750f, "This value, as a multiplier (1 is 1x, 2 is 2x, ...), is applied to the player's base damage if their black hole cannot kill an enemy, boss or otherwise. It is recommended to make this value relatively high.");

		}
	}
}