using BepInEx;
using Rewired.Data.Mapping;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior;
using Xan.ROR2VoidPlayerCharacterCommon.EntityStates;

namespace Xan.ROR2VoidPlayerCharacterCommon.Registration {

	/// <summary>
	/// Mods that want to use Void damage related stuffs (such as void immunity) register themselves here.
	/// </summary>
	internal static class VoidBehaviorRegistry {

		internal static Dictionary<BodyIndex, BaseUnityPlugin> _immunityRegistrars = new Dictionary<BodyIndex, BaseUnityPlugin>();
		internal static readonly Dictionary<BodyIndex, bool> _nativeVoidConditionalImmunitySubjects = new Dictionary<BodyIndex, bool>(); // For mod internals only!
		
		internal static bool IsImmuneToVoidFog(BodyIndex bodyIndex) {
			if (_immunityRegistrars.ContainsKey(bodyIndex)) {
				if (_nativeVoidConditionalImmunitySubjects.ContainsKey(bodyIndex)) {
					return Configuration.EnforceVoidNativeImmunity;
				}
				return true;
			}
			return false;
		}

		internal static void RegisterForVoidImmunities(BaseUnityPlugin registrar, BodyIndex bodyIndex) {
			if (registrar == null) throw new ArgumentNullException(nameof(registrar));
			if (!BodyCatalog.availability.available) {
				throw new InvalidOperationException($"{nameof(RegisterForVoidImmunities)} MUST be called strictly after all CharacterBody instances are available. You can ensure this by calling this upon the execution of RoR2.BodyCatalog.availability.onAvailable");
			}
			if (bodyIndex == BodyIndex.None) throw new ArgumentOutOfRangeException(nameof(bodyIndex), "The provided body index is BodyIndex.None");

			if (bodyIndex == 0) {
				Log.LogWarning($"The body index input into {nameof(VoidBehaviorRegistry)}.{nameof(RegisterForVoidImmunities)} was 0. Chances are you didn't want this, unless you are making the Acid Larva immune to the fog.");
			}
			if (_immunityRegistrars.ContainsKey(bodyIndex)) {
				throw new InvalidOperationException($"Immunity was already registered for {Helpers.BodyToString(bodyIndex)} by mod: {Helpers.ModToString(_immunityRegistrars[bodyIndex])}");
			}
			Log.LogTrace($"Registered {Helpers.BodyToString(bodyIndex)} to be immune to Void Fog.");
			_immunityRegistrars[bodyIndex] = registrar;
		}

		internal static bool VerifyProperConstruction(CharacterBody body) {
			CharacterDeathBehavior deathBehavior = body.GetComponent<CharacterDeathBehavior>();
			bool ok = true;
			if (deathBehavior == null) {
				Log.LogError($"CharacterBody [{body.GetDisplayName()}] ({body.name} // {body.baseNameToken}) does not have a death behavior!");
				ok = false;
			}
			if (deathBehavior.deathState.stateType.GetInterface(nameof(IHasDelayedGameOver)) != null) {
				Log.LogInfo($"CharacterBody [{body.GetDisplayName()}] ({body.name} // {body.baseNameToken}) has a death state that delays the Game Over screen, verifying ...");
				if (body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToVoidDeath)) {
					Log.LogError($"... it is immune to Void Death! This WILL cause problems. Void Characters automatically reject Void Death caused by themselves.");
					ok = false;
				}
			}
			return ok;
		}
	}
}
