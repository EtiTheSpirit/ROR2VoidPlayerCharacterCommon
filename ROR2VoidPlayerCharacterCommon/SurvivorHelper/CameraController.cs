using Rewired.Utils.Libraries.TinyJson;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {

	/// <summary>
	/// Controls the camera offsets.
	/// </summary>
	public class CameraController : MonoBehaviour {

		/// <summary>
		/// Sets the camera's offset.
		/// </summary>
		[DoNotSerialize]
		public Func<Vector3> getCameraOffset;

		/// <summary>
		/// Sets the camera's vertical pivot point offset.
		/// </summary>
		[DoNotSerialize]
		public Func<float> getCameraPivot;

		/// <summary>
		/// Sets whether or not the client is using a full size character.
		/// </summary>
		[DoNotSerialize]
		public Func<bool> getUseFullSizeCharacter;

		private CameraTargetParams _camTarget;

		private void Awake() {
			_camTarget = GetComponent<CameraTargetParams>();
		}

		private void Update() {
			if (_camTarget && (getCameraOffset != null) && (getCameraPivot != null) && (getUseFullSizeCharacter != null)) {
				if (!_camTarget.cameraParams) _camTarget.cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
				Vector3 pos = getCameraOffset();
				float vert = getCameraPivot();
				if (!getUseFullSizeCharacter()) {
					pos *= 0.5f;
					vert *= 0.5f;
				}
				_camTarget.cameraParams.data.idealLocalCameraPos = pos;
				_camTarget.cameraParams.data.pivotVerticalOffset = vert;
			}
		}

	}
}
