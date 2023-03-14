using BepInEx;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior;
using Xan.ROR2VoidPlayerCharacterCommon.Registration;

namespace Xan.ROR2VoidPlayerCharacterCommon {

	/// <summary>
	/// All API members for you to use in the code that involve getting and registering information reside here.
	/// </summary>
	public static class XanVoidAPI {

		/// <summary>
		/// Returns whether or not the provided <see cref="BodyIndex"/> has been registered as one that is immune to void fog.
		/// </summary>
		/// <param name="bodyIndex"></param>
		/// <returns></returns>
		public static bool IsImmuneToVoidFog(BodyIndex bodyIndex) => VoidBehaviorRegistry.IsImmuneToVoidFog(bodyIndex);

		/// <summary>
		/// Returns true if the provided <see cref="DamageInfo"/> should cause the void death visual effect.
		/// </summary>
		/// <param name="damage"></param>
		/// <returns></returns>
		public static bool ShouldShowVoidDeath(DamageInfo damage) {
			bool isBlacklisted = damage.HasModdedDamageType(VoidDamageTypes.BlacklistExaggeratedVoidDeath);
			if (damage.damageType.HasFlag(DamageType.VoidDeath)) return !isBlacklisted;
			if (damage.HasModdedDamageType(VoidDamageTypes.VoidDeathOnDeath)) {
				if (isBlacklisted) {
					Log.LogError("Malformed damage type; someone has flagged the damage type as one to cause the Lost Seer's Lenses effect, but also gave it the type telling it to *not* do this! Make up your mind!");
					Log.LogError((new StackTrace()).ToString());
					Log.LogWarning($"Attacker: {damage.attacker}");
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Register this character to be immune to Void Fog. This is both the visual effect and the damage at once. This <strong>MUST</strong> be called after the body catalog is available! See <see cref="BodyCatalog.availability"/> for more information (consider using its event).
		/// </summary>
		/// <param name="registrar">Your mod's plugin class. This is used to keep track of who did what.</param>
		/// <param name="bodyIndex">The <see cref="CharacterBody"/> to register, by its index.</param>
		public static void RegisterForVoidImmunities(BaseUnityPlugin registrar, BodyIndex bodyIndex) => VoidBehaviorRegistry.RegisterForVoidImmunities(registrar, bodyIndex);

		/// <summary>
		/// Call this <strong>AFTER</strong> registering your survivor. The input body should be its prefab. This will output information about the character into the console, making sure everything is set up properly.
		/// </summary>
		/// <param name="body"></param>
		public static bool VerifyProperConstruction(CharacterBody body) => VoidBehaviorRegistry.VerifyProperConstruction(body);
	}
}
