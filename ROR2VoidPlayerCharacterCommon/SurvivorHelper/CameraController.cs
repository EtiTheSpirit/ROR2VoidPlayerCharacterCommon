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
		public Func<Vector3> getCameraOffset;

		/// <summary>
		/// Sets the camera's vertical pivot point offset.
		/// </summary>
		public Func<float> getCameraPivot;

		/// <summary>
		/// Sets whether or not the client is using a full size character.
		/// </summary>
		public Func<bool> getUseFullSizeCharacter;

		/// <summary>
		/// Whether or not this <see cref="CameraController"/> was added to a full size character. Used to prevent the camera from changing.
		/// </summary>
		private bool _isThisInstanceFullSizeCharacter;
		private bool _setFullSizeBool;

		private CameraTargetParams _camTarget;

		private void Awake() {
			_camTarget = GetComponent<CameraTargetParams>();
		}

		private void Update() {
			if (_camTarget && (getCameraOffset != null) && (getCameraPivot != null) && (getUseFullSizeCharacter != null)) {
				if (!_setFullSizeBool) {
					_setFullSizeBool = true;
					_isThisInstanceFullSizeCharacter = getUseFullSizeCharacter();
				}
				if (!_camTarget.cameraParams) _camTarget.cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
				Vector3 pos = getCameraOffset();
				float vert = getCameraPivot();
				if (!_isThisInstanceFullSizeCharacter) {
					pos *= 0.5f;
					vert *= 0.5f;
				}
				_camTarget.cameraParams.data.idealLocalCameraPos = pos;
				_camTarget.cameraParams.data.pivotVerticalOffset = vert;
			}
		}

	}
}
