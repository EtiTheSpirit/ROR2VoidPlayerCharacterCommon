using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior {

	/// <summary>
	/// A registry of VFX and Projectiles that are used.
	/// </summary>
	public static class VoidEffects {
		
		/// <summary>
		/// A variation of the crit goggles kill effect that does not emit a sound. This is not immediately set and may be null if referenced in a mod's init cycle.
		/// </summary>
		public static GameObject SilentVoidCritDeathEffect { get; private set; }

		internal static void Initialize() {
			On.RoR2.HealthComponent.AssetReferences.Resolve += InterceptHealthAssetsResolver;
		}

		private static void InterceptHealthAssetsResolver(On.RoR2.HealthComponent.AssetReferences.orig_Resolve originalMethod) {
			originalMethod();
			SilentVoidCritDeathEffect = PrefabAPI.InstantiateClone(HealthComponent.AssetReferences.critGlassesVoidExecuteEffectPrefab, "SilentExaggeratedVoidDeathFX");
			SilentVoidCritDeathEffect.AddComponent<NetworkIdentity>();
			EffectComponent fx = SilentVoidCritDeathEffect.GetComponentInChildren<EffectComponent>();
			fx.soundName = null;
			ContentAddition.AddEffect(SilentVoidCritDeathEffect);
			On.RoR2.HealthComponent.AssetReferences.Resolve -= InterceptHealthAssetsResolver; // Clean up!
			Log.LogTrace("Instantiated prefab for silent void crit death effect.");
		}
	}
}
