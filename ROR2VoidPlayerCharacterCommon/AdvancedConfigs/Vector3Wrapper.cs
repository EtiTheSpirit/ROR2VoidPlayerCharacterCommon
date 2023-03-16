using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {

	/// <summary>
	/// A wrapper for <see cref="Vector3"/> that is designed for ROO.
	/// </summary>
	public class Vector3Wrapper {

		/// <summary>
		/// The X component of this vector.
		/// </summary>
		public float X {
			get => _x.Value;
			set => _x.Value = value;
		}

		/// <summary>
		/// The Y component of this vector.
		/// </summary>
		public float Y {
			get => _y.Value;
			set => _y.Value = value;
		}

		/// <summary>
		/// The Z component of this vector.
		/// </summary>
		public float Z {
			get => _z.Value;
			set => _z.Value = value;
		}

		/// <summary>
		/// A complete <see cref="Vector3"/> composed from the config options.
		/// </summary>
		public Vector3 Value {
			get => new Vector3(X, Y, Z);
			set {
				X = value.x;
				Y = value.y;
				Z = value.z;
			}
		}

		private readonly ConfigEntry<float> _x;
		private readonly ConfigEntry<float> _y;
		private readonly ConfigEntry<float> _z;

		private const string DESCRIBE_VALUE = "This value serves as the {0} component of the 3D coordinate.";
		private const string DESCRIBE_X = "X (+right/-left)";
		private const string DESCRIBE_Y = "Y (+up/-down)";
		private const string DESCRIBE_Z = "Z (+forward/-backward)";

		/// <summary>
		/// Creates a <see cref="Vector3Wrapper"/> which creates 3 float values, registers them to Risk of Options, and then allows accessing each component via properties.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		public Vector3Wrapper(AdvancedConfigBuilder parent, string name, Vector3 @default, string description, Vector3 min = default, Vector3? max = null, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired) {
			Vector3 realMax = max ?? new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			_x = parent.Bind($"{name} (X)", @default.x, $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_X)}", min.x, realMax.x, null, restartRequired, "{0}m", null, true);
			_y = parent.Bind($"{name} (Y)", @default.x, $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Y)}", min.y, realMax.y, null, restartRequired, "{0}m", null, true);
			_z = parent.Bind($"{name} (Z)", @default.x, $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Z)}", min.z, realMax.z, null, restartRequired, "{0}m", null, true);
		}
	}
}
