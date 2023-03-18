using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using static RiskOfOptions.OptionConfigs.BaseOptionConfig;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {
	
	/// <summary>
	/// Contains predicates for use in ROO.
	/// </summary>
	public static class CommonPredicates {

		/// <summary>
		/// Returns true if the player is the host, or if there is no active run.
		/// </summary>
		public static readonly IsDisabledDelegate ONLY_ENABLE_AS_HOST = () => !NetworkServer.active && Run.instance != null;

	}
}
