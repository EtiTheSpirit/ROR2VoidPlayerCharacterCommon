using R2API;
using RoR2;
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
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject NullifierImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default jailer black hole, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject JailerImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default devastator black hole, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject DevastatorImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default devastator black hole bomblet, but with <see cref="VoidDamageTypes.ConditionalVoidDeath"/> applied.
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject DevastatorBomblet { get; private set; }

		/// <summary>
		/// The same thing as the default reaver black hole, but completely incapable of doing Void Death by applying <see cref="VoidDamageTypes.NeverVoidDeath"/>
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject NoInstakillNullifierImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default jailer black hole, but completely incapable of doing Void Death by applying <see cref="VoidDamageTypes.NeverVoidDeath"/>
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject NoInstakillJailerImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default devastator black hole, but completely incapable of doing Void Death by applying <see cref="VoidDamageTypes.NeverVoidDeath"/>
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject NoInstakillDevastatorImplosion { get; private set; }

		/// <summary>
		/// The same thing as the default devastator black hole bomblet, but completely incapable of doing Void Death by applying <see cref="VoidDamageTypes.NeverVoidDeath"/>
		/// <para/>
		/// <strong>IMPORTANT:</strong> When firing this, you <em>must</em> set the fallback damage of the projectile yourself (make use of <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>)!
		/// </summary>
		public static GameObject NoInstakillDevastatorBomblet { get; private set; }

		internal static void Initialize() {
			Log.LogMessage("Instantiating conditional Void death implosions...");
			Log.LogTrace("Conditional Reaver...");
			NullifierImplosion = RegisterVoidImplosion("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab", "NullifierImplosionConditional", false);
			Log.LogTrace("Conditional Jailer...");
			JailerImplosion = RegisterVoidImplosion("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab", "JailerImplosionConditional", false);
			Log.LogTrace("Conditional Devastator...");
			DevastatorImplosion = RegisterVoidImplosion("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab", "DevastatorImplosionConditional", false);
			Log.LogTrace("Conditional Devastator Bomblet...");
			DevastatorBomblet = RegisterVoidImplosion("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsProjectile.prefab", "DevastatorBombletConditional", false);

			Log.LogTrace("No-Instakill Reaver...");
			NoInstakillNullifierImplosion = RegisterVoidImplosion("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab", "NullifierImplosionNoVoidDeath", true);
			Log.LogTrace("No-Instakill Jailer...");
			NoInstakillJailerImplosion = RegisterVoidImplosion("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab", "JailerImplosionNoVoidDeath", true);
			Log.LogTrace("No-Instakill Devastator...");
			NoInstakillDevastatorImplosion = RegisterVoidImplosion("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab", "DevastatorImplosionNoVoidDeath", true);
			Log.LogTrace("No-Instakill Devastator Bomblet...");
			NoInstakillDevastatorBomblet = RegisterVoidImplosion("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsProjectile.prefab", "DevastatorBombletNoVoidDeath", true);
		}

		private static GameObject RegisterVoidImplosion(string address, string newName, bool isNeverKill) {
			GameObject instance = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion(), newName, true);
			DamageAPI.ModdedDamageTypeHolderComponent damageHolder = instance.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
			damageHolder.Add(VoidDamageTypes.DisplayVoidDeathOnKill);
			if (isNeverKill) {
				damageHolder.Add(VoidDamageTypes.NeverVoidDeath);
				instance.GetComponent<ProjectileDamage>().damageType &= ~DamageType.VoidDeath;
			} else {
				damageHolder.Add(VoidDamageTypes.ConditionalVoidDeath);
			}
			return instance;
		}

	}
}
