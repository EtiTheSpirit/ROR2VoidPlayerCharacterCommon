using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon {

	/// <summary>
	/// All customizations for this mod are here.
	/// </summary>
	public static class Configuration {

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

		#region Backing Fields

		[AllowNull]
		private static ConfigEntry<bool> _traceLogging;

		[AllowNull]
		private static ConfigEntry<bool> _enforceNativeImmunity;

		[AllowNull]
		private static ConfigEntry<bool> _disableMufflerOnDeath;

		#endregion

		internal static void Initialize(ConfigFile cfg) {
			Log.LogMessage("Initializing Configuration...");
			_traceLogging = cfg.Bind("Mod Meta", "Trace Logging", false, "Trace Logging is a form of precise logging that tracks every little thing that is being done. Most of this information is useless outside of debugging the mod. Consider enabling it when forming bug reports.");
			_enforceNativeImmunity = cfg.Bind("Void Behavior", "Enforce Immunity on Void Creatures", false, "If enabled, all void(touched) enemies and allies will be immune to damage from the void fog and atmosphere. This resolves a few weird edge cases where void enemies can take damage from the atmosphere.");
			_disableMufflerOnDeath = cfg.Bind("Void Behavior", "Disable Low Pass Filter on Death", true, "If enabled, and if the current player has a void death animation, this will disable the low pass filter upon death so that the sound effects of the black hole are not affected.");
		}
	}
}