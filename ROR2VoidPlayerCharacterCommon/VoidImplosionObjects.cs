using R2API;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior;

namespace Xan.ROR2VoidPlayerCharacterCommon {

	/// <summary>
	/// This class stores the implosion effects for all void deaths.
	/// </summary>
	public static class VoidImplosionObjects {

		/// <summary>
		/// The same thing as the default reaver black hole, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// </summary>
		public static GameObject NullifierImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default jailer black hole, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// </summary>
		public static GameObject JailerImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default devastator black hole, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// </summary>
		public static GameObject DevastatorImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default devastator black hole bomblet, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// </summary>
		public static GameObject DevastatorBomblet { get; private set; }

		internal static void Initialize() {
			Log.LogMessage("Instantiating conditional Void death implosions...");
			Log.LogTrace("Reaver...");
			NullifierImplosion = RegisterVoidImplosion("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab", "NullifierImplosionConditional");
			Log.LogTrace("Jailer...");
			JailerImplosion = RegisterVoidImplosion("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab", "JailerImplosionConditional");
			Log.LogTrace("Devastator...");
			DevastatorImplosion = RegisterVoidImplosion("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab", "DevastatorImplosionConditional");
			Log.LogTrace("Devastator Bomblet...");
			DevastatorBomblet = RegisterVoidImplosion("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsProjectile.prefab", "DevastatorBombletConditional");
		}

		private static GameObject RegisterVoidImplosion(string address, string newName) {
			GameObject instance = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion(), newName, true);
			instance.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(VoidDamageTypes.ConditionalVoidDeath);
			return instance;
		}

	}
}
