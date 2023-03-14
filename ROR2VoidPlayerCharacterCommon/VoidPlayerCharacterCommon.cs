using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xan.ROR2VoidPlayerCharacterCommon {
	[BepInDependency(R2API.R2API.PluginGUID)]
	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	[R2APISubmoduleDependency(nameof(DamageAPI))]
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
	public class VoidPlayerCharacterCommon : BaseUnityPlugin {
		public const string PLUGIN_GUID = PLUGIN_AUTHOR + "." + PLUGIN_NAME;
		public const string PLUGIN_AUTHOR = "Xan";
		public const string PLUGIN_NAME = "VoidPlayerCharacterCommon";
		public const string PLUGIN_VERSION = "1.0.0";
		
		public void Awake() {
			Log.Init(Logger);
		}

	}
}
