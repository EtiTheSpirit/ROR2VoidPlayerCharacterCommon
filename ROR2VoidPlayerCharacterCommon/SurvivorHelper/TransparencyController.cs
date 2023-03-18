using BepInEx.Configuration;
using Rewired.Utils.Libraries.TinyJson;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {

	/// <summary>
	/// This component contains code that manages the visible transparency of a character. It should be added to the same object as a <see cref="CharacterBody"/>.
	/// </summary>
	public class TransparencyController : MonoBehaviour {
		
		/// <summary>
		/// Set this to the applicable mod's "Transparency In Combat" setting.
		/// </summary>
		[DoNotSerialize]
		public Func<int> getTransparencyInCombat;

		/// <summary>
		/// Set this to the applicable mod's "Transparency Out Of Combat" setting.
		/// </summary>
		[DoNotSerialize]
		public Func<int> getTransparencyOutOfCombat;

		/// <summary>
		/// Returns the CharacterBody of the client player running this instance of the game. May be null.
		/// </summary>
		private CharacterBody ClientPlayerBody {
			get {
				LocalUser client = LocalUserManager.GetFirstLocalUser();
				if (client != null) {
					return client.cachedBody;
				}
				return null;
			}
		}

		private void Awake() {
			_body = GetComponent<CharacterBody>();
			_renderers = GetComponentsInChildren<Renderer>();
			_propertyStorage = new MaterialPropertyBlock();
			SceneCamera.onSceneCameraPreRender += OnSceneCameraPreRender;

		}

		private void OnDestroy() {
			try {
				SceneCamera.onSceneCameraPreRender -= OnSceneCameraPreRender;
				getTransparencyInCombat = null;
				getTransparencyOutOfCombat = null;
			} catch { }
		}

		private void DestructionSequence() {
			SceneCamera.onSceneCameraPreRender -= OnSceneCameraPreRender;
			DestroyImmediate(this);
		}

		private bool DestroyIfNeeded() {
			if (!this) {
				Log.LogTrace("If you are seeing this, something horribly wrong has happened.");
				return true;
			}
			if (!_body) {
				Log.LogTrace($"Destroyed {nameof(TransparencyController)}: The {nameof(CharacterBody)} it is operating alongside has been destroyed.");
				DestructionSequence();
				return true;
			}
			if (!gameObject) {
				Log.LogTrace($"Destroyed {nameof(TransparencyController)}: The object it is attached to has been destroyed.");
				DestructionSequence();
				return true;
			}
			if (_body && _isMine) {
				if (_body.gameObject != gameObject) {
					Log.LogTrace($"Destroyed {nameof(TransparencyController)}: The body it is operating is present, but the GameObject it is attached to is not the same as this one.");
					DestructionSequence();
					return true;
				}
			}
			return false;
		}

		private void FixedUpdate() {
			_isMine = ClientPlayerBody == _body;
			if (!_isMine) return;
			if (DestroyIfNeeded()) return;
			if (getTransparencyInCombat == null || getTransparencyOutOfCombat == null) return;

			if (_body.outOfDanger && _body.outOfCombat) {
				SetTransparency(getTransparencyOutOfCombat() / 100f);
			} else {
				SetTransparency(getTransparencyInCombat() / 100f);
			}

		}

		private void OnSceneCameraPreRender(SceneCamera _) {
			if (DestroyIfNeeded()) return;

			for (int index = 0; index < _renderers.Length; index++) {
				Renderer ren = _renderers[index];
				UpdateSingle(ren);
			}
		}

		private bool UpdateSingle(Renderer renderer) {
			if (!renderer) return false;
			if (!renderer.isVisible) return false;
			try {
				renderer.GetPropertyBlock(_propertyStorage);
				_propertyStorage.SetFloat("_Fade", _currentOpacity);
				renderer.SetPropertyBlock(_propertyStorage);
				return true;
			} catch { }
			return false;
		}

		/// <summary>
		/// Change the displayed transparency of the body.
		/// </summary>
		/// <param name="transparency"></param>
		private void SetTransparency(float transparency) {
			_currentOpacity = 1f - transparency;
		}

		private CharacterBody _body;

		private Renderer[] _renderers;

		private MaterialPropertyBlock _propertyStorage;

		private float _currentOpacity = 1f;

		private bool _isMine;

	}
}
