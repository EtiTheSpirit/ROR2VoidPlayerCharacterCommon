using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {
	/// <summary>
	/// A wrapper for <see cref="Vector2"/> that is designed for ROO.
	/// </summary>
	public class ReplicatedMinMaxWrapper : IReplicatedConfigContainer {

		/// <summary>
		/// The minimum value of this range.
		/// </summary>
		public float Min => Mathf.Min(_x.Value, _y.Value);

		/// <summary>
		/// The maxiomum value of this range.
		/// </summary>
		public float Max => Mathf.Max(_x.Value, _y.Value);

		/// <summary>
		/// Returns this as a <see cref="Vector2"/> composed from (<see cref="Min"/>, <see cref="Max"/>).
		/// </summary>
		public Vector2 Vector => new Vector2(Min, Max);

		/// <summary>
		/// Changes the min/max values.
		/// </summary>
		/// <param name="min">The minimum value. This <strong>must</strong> be less than or equal to <paramref name="max"/>!</param>
		/// <param name="max">The maximum value. This <strong>must</strong> be greater than or equal to <paramref name="min"/>!</param>
		/// <exception cref="ArgumentOutOfRangeException">If you failed to read the previous line and made min &gt; max.</exception>
		public void SetRange(float min, float max) {
			if (min > max) throw new ArgumentOutOfRangeException(nameof(min), "The minimum cannot be larger than the maximum, d u m m y");
			_x.LocalValue = min;
			_y.LocalValue = max;
		}

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _x;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _y;

		private const string DESCRIBE_VALUE = "This value serves as the {0} component of the allowed range of values. If the minimum is larger than the maximum, their roles will be swapped (effectively, the larger of the two is always treated as the maximum).";
		private const string DESCRIBE_X = "minimum";
		private const string DESCRIBE_Y = "maximum";

		/// <summary>
		/// Creates a <see cref="ReplicatedMinMaxWrapper"/> which creates 2 float values, registers them to Risk of Options, and then allows accessing each component via properties.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="defaultMin"></param>
		/// <param name="defaultMax"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		public ReplicatedMinMaxWrapper(AdvancedConfigBuilder parent, string name, string description, float defaultMin, float defaultMax, float min = 0, float max = float.MaxValue, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired, string formatString = "{0}") {
			_x = (ReplicatedConfigEntry<float>)parent.BindFloatInternal($"{name} (Min)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_X)}", defaultMin, min, max, null, restartRequired, formatString, CommonPredicates.ONLY_ENABLE_AS_HOST, true, true);
			_y = (ReplicatedConfigEntry<float>)parent.BindFloatInternal($"{name} (Max)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Y)}", defaultMax, min, max, null, restartRequired, formatString, CommonPredicates.ONLY_ENABLE_AS_HOST, true, true);
			_x.SettingChanged += (_, _) => SettingChanged?.Invoke(Min, Max);
			_y.SettingChanged += (_, _) => SettingChanged?.Invoke(Min, Max);
		}

		/// <summary>
		/// Returns true if the provided <see cref="ConfigEntryBase"/> is one of the internal backing configs for this object.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool BelongsToThis(ConfigEntryBase toCheck) {
			return toCheck == _x.BaseLocalBackingConfig || toCheck == _y.BaseLocalBackingConfig;
		}

		/// <summary>
		/// Fires when any of the two values of this entry change.
		/// </summary>
		public event Action<float, float> SettingChanged;

	}

	/// <summary>
	/// A wrapper for <see cref="Vector2"/> that is designed for ROO.
	/// </summary>
	public class LocalMinMaxWrapper : IReplicatedConfigContainer {

		/// <summary>
		/// The minimum value of this range.
		/// </summary>
		public float Min => Mathf.Min(_x.Value, _y.Value);

		/// <summary>
		/// The maxiomum value of this range.
		/// </summary>
		public float Max => Mathf.Max(_x.Value, _y.Value);

		/// <summary>
		/// Returns this as a <see cref="Vector2"/> composed from (<see cref="Min"/>, <see cref="Max"/>).
		/// </summary>
		public Vector2 Vector => new Vector2(Min, Max);

		/// <summary>
		/// Changes the min/max values.
		/// </summary>
		/// <param name="min">The minimum value. This <strong>must</strong> be less than or equal to <paramref name="max"/>!</param>
		/// <param name="max">The maximum value. This <strong>must</strong> be greater than or equal to <paramref name="min"/>!</param>
		/// <exception cref="ArgumentOutOfRangeException">If you failed to read the previous line and made min &gt; max.</exception>
		public void SetRange(float min, float max) {
			if (min > max) throw new ArgumentOutOfRangeException(nameof(min), "The minimum cannot be larger than the maximum, d u m m y");
			_x.Value = min;
			_y.Value = max;
		}
		private readonly ConfigEntry<float> _x;

		private readonly ConfigEntry<float> _y;

		private const string DESCRIBE_VALUE = "This value serves as the {0} component of the allowed range of values. If the minimum is larger than the maximum, their roles will be swapped (effectively, the larger of the two is always treated as the maximum).";
		private const string DESCRIBE_X = "minimum";
		private const string DESCRIBE_Y = "maximum";

		/// <summary>
		/// Creates a <see cref="LocalMinMaxWrapper"/> which creates 2 float values, registers them to Risk of Options, and then allows accessing each component via properties.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="defaultMin"></param>
		/// <param name="defaultMax"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		public LocalMinMaxWrapper(AdvancedConfigBuilder parent, string name, string description, float defaultMin, float defaultMax, float min = 0, float max = float.MaxValue, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired, string formatString = "{0}") {
			_x = (ConfigEntry<float>)parent.BindFloatInternal($"{name} (Min)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_X)}", defaultMin, min, max, null, restartRequired, formatString, null, true, false);
			_y = (ConfigEntry<float>)parent.BindFloatInternal($"{name} (Max)", $"{description}\n\n{string.Format(DESCRIBE_VALUE, DESCRIBE_Y)}", defaultMax, min, max, null, restartRequired, formatString, null, true, false);
			_x.SettingChanged += (_, _) => SettingChanged?.Invoke(Min, Max);
			_y.SettingChanged += (_, _) => SettingChanged?.Invoke(Min, Max);
		}

		/// <summary>
		/// Returns true if the provided <see cref="ConfigEntryBase"/> is one of the internal backing configs for this object.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool BelongsToThis(ConfigEntryBase toCheck) {
			return toCheck == _x || toCheck == _y;
		}

		/// <summary>
		/// Fires when any of the two values of this entry change.
		/// </summary>
		public event Action<float, float> SettingChanged;
	}

}
