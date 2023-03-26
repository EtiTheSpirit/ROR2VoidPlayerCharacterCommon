using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Xan.ROR2VoidPlayerCharacterCommon.ProjectileHelper {

	/// <summary>
	/// Attach to a rigidbody to give it fake gravity. The reason this exists is so that the gravity can be configured per instance.
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class FakeGravity : MonoBehaviour {

		/// <summary>
		/// The vertical gravity of this object in world space (negative values go down).
		/// </summary>
		public float gravity = Physics.gravity.y;
		private Rigidbody _rigidBody;

		void Awake() {
			_rigidBody = GetComponent<Rigidbody>();
		}

		void FixedUpdate() {
			_rigidBody.useGravity = false;
			_rigidBody.AddForce(0, gravity, 0);
		}

	}
}
