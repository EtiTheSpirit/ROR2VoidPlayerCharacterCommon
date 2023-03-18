using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {

	/// <summary>
	/// A wrapper for <see cref="Vector3"/> that is designed for ROO.
	/// </summary>
	public class ReplicatedVector3Wrapper : IReplicatedConfigContainer {

		/// <summary>
		/// The X component of this vector.
		/// </summary>
		public float X {
			get => _x.Value;
			set => _x.LocalValue = value;
		}

		/// <summary>
		/// The Y component of this vector.
		/// </summary>
		public float Y {
			get => _y.Value;
			set => _y.LocalValue = value;
		}

		/// <summary>
		/// The Z component of this vector.
		/// </summary>
		public float Z {
			get => _z.Value;
			set => _z.LocalValue = value;
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

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _x;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _y;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _z;

		private const string DESCRIBE_VALUE = "This value serves as the {0} component of the 3D coordinate.";
		private const string DESCRIBE_X = "X (+right/-left)";
		private const string DESCRIBE_Y = "Y (+up/-down)";
		private const string DESCRIBE_Z = "Z (+forward/-backward)";

		/// <summary>
		/// Creates a <see cref="ReplicatedVector3Wrapper"/> which creates 3 float values, registers them to Risk of Options, and then allows accessing each component via properties.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// 
		public ReplicatedVector3Wrapper(AdvancedConfigBuilder parent, string name, string description, Vector3 @default, Vector3 min = default, Vector3? max = null, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired, string formatString = "{0}m") {
			Vector3 realMax = max ?? new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			_x = (ReplicatedConfigEntry<float>)parent.BindFloatInternal($"{name} (X)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_X)}", @default.x, min.x, realMax.x, null, restartRequired, formatString, CommonPredicates.ONLY_ENABLE_AS_HOST, true, true);
			_y = (ReplicatedConfigEntry<float>)parent.BindFloatInternal($"{name} (Y)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Y)}", @default.y, min.y, realMax.y, null, restartRequired, formatString, CommonPredicates.ONLY_ENABLE_AS_HOST, true, true);
			_z = (ReplicatedConfigEntry<float>)parent.BindFloatInternal($"{name} (Z)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Z)}", @default.z, min.z, realMax.z, null, restartRequired, formatString, CommonPredicates.ONLY_ENABLE_AS_HOST, true, true);
			_x.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
			_y.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
			_z.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
		}

		/// <summary>
		/// Returns true if the provided <see cref="ConfigEntryBase"/> is one of the internal backing configs for this object.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool BelongsToThis(ConfigEntryBase toCheck) {
			return toCheck == _x.BaseLocalBackingConfig || toCheck == _y.BaseLocalBackingConfig || toCheck == _z.BaseLocalBackingConfig;
		}

		/// <summary>
		/// Fires when any of the three coordinates of this entry change.
		/// </summary>
		public event Action<Vector3> SettingChanged;
	}

	/// <summary>
	/// A wrapper for <see cref="Vector3"/> that is designed for ROO.
	/// </summary>
	public class LocalVector3Wrapper : IReplicatedConfigContainer {

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
		/// Creates a <see cref="ReplicatedVector3Wrapper"/> which creates 3 float values, registers them to Risk of Options, and then allows accessing each component via properties.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// 
		public LocalVector3Wrapper(AdvancedConfigBuilder parent, string name, string description, Vector3 @default, Vector3 min = default, Vector3? max = null, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired, string formatString = "{0}m") {
			Vector3 realMax = max ?? new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			_x = (ConfigEntry<float>)parent.BindFloatInternal($"{name} (X)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_X)}", @default.x, min.x, realMax.x, null, restartRequired, formatString, null, true, false);
			_y = (ConfigEntry<float>)parent.BindFloatInternal($"{name} (Y)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Y)}", @default.y, min.y, realMax.y, null, restartRequired, formatString, null, true, false);
			_z = (ConfigEntry<float>)parent.BindFloatInternal($"{name} (Z)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Z)}", @default.z, min.z, realMax.z, null, restartRequired, formatString, null, true, false);
			_x.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
			_y.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
			_z.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
		}

		/// <summary>
		/// Returns true if the provided <see cref="ConfigEntryBase"/> is one of the internal backing configs for this object.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool BelongsToThis(ConfigEntryBase toCheck) {
			return toCheck == _x || toCheck == _y || toCheck == _z;
		}

		/// <summary>
		/// Fires when any of the three coordinates of this entry change.
		/// </summary>
		public event Action<Vector3> SettingChanged;
	}
}
