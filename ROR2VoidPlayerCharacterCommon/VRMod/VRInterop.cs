using BepInEx.Configuration;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRAPI;

namespace Xan.ROR2VoidPlayerCharacterCommon.VRMod {

	/// <summary>
	/// This class provides interoperability with DrBibop's VR mod.
	/// </summary>
	public static class VRInterop {

		/// <summary>
		/// True if the VR mod is available and installed. VR API members <strong>must not</strong> be used if this is <see langword="false"/>.
		/// </summary>
		public static bool VRAvailable => VR.enabled && MotionControls.enabled;

		/// <summary>
		/// Returns whether or not to perform additional aim compensation for VR players. This may not be implemented.
		/// </summary>
		/// <param name="cfg"></param>
		/// <returns></returns>
		public static bool DoVRAimCompensation(ConfigEntry<bool> cfg) {
			if (!VRAvailable) return false;
			return cfg.Value;
		}

		/// <summary>
		/// Returns true if, assuming this is called in the context of an effect that should not play in VR, the effect can show (either due to the player being remote, or due to VR being off)
		/// </summary>
		/// <returns></returns>
		public static bool CanShowNonVREffect(CharacterBody body) {
			if (!VRAvailable) return true; // Can show, no VR
			return !IsVRLocalPlayer(body); // Only show if it's *not* the local player in VR.
		}

		/// <summary>
		/// True if the given <see cref="CharacterBody"/> is associated with the client player, and this client is in VR.
		/// </summary>
		/// <param name="body"></param>
		/// <returns></returns>
		public static bool IsVRLocalPlayer(CharacterBody body) {
			if (!VRAvailable) return false;
			return body.IsUsingMotionControls(); // Checks IsVR()
		}

		/// <summary>
		/// Returns the aim ray of the dominant hand. The dominant hand controls the primary weapon.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public static Ray GetDominantHandRay(IAimRayProvider state) {
			if (!VRAvailable) return state.PublicAimRay;
			return MotionControls.dominantHand.aimRay;
		}

		/// <summary>
		/// Returns the aim ray of the non-dominant hand. The non-dominant hand controls the secondary weapon.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public static Ray GetNonDominantHandRay(IAimRayProvider state) {
			if (!VRAvailable) return state.PublicAimRay;
			return MotionControls.nonDominantHand.aimRay;
		}

		/// <summary>
		/// This interface must be implemented on any <see cref="BaseState"/> that makes use of VR, to expose its <see cref="BaseState.GetAimRay"/> method.
		/// Implementors should override <see cref="BaseState.GetAimRay"/> to return either <see cref="GetDominantHandRay(IAimRayProvider)"/> or <see cref="GetNonDominantHandRay(IAimRayProvider)"/>.
		/// </summary>
		public interface IAimRayProvider {
			
			/// <summary>
			/// Returns a publicly available copy of the default, non-VR aim ray, acquired by calling <see langword="base"/>.GetAimRay in a class extending <see cref="BaseState"/>
			/// </summary>
			public Ray PublicAimRay { get; }

		}
	}
}
