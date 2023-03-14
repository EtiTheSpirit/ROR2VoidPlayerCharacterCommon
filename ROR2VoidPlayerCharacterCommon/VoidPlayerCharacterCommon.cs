using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior;

namespace Xan.ROR2VoidPlayerCharacterCommon {

	[BepInDependency(R2API.R2API.PluginGUID)]
	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	[R2APISubmoduleDependency(nameof(DamageAPI), nameof(PrefabAPI))]
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
	internal class VoidPlayerCharacterCommon : BaseUnityPlugin {
		public const string PLUGIN_GUID = PLUGIN_AUTHOR + "." + PLUGIN_NAME;
		public const string PLUGIN_AUTHOR = "Xan";
		public const string PLUGIN_NAME = "VoidPlayerCharacterCommon";
		public const string PLUGIN_VERSION = "1.1.0";

		[AllowNull]
		internal static VoidPlayerCharacterCommon Instance { get; private set; }

		internal void Awake() {
			Instance = this;
			Log.Init(Logger);
			Log.LogMessage("Creating Void Player Character Common API...");
			Configuration.Initialize(Config);
			VoidDamageTypes.Initialize();
			VoidDamageHooks.Initialize();
			VoidEffects.Initialize();
			VoidImplosionObjects.Initialize();
		}

	}
}
