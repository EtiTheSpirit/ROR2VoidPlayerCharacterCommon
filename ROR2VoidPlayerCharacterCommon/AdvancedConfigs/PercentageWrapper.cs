using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {

	/// <summary>
	/// This class wraps a floating point config entry ranging from 0 to 100 such that it functions like a percentage in code, but a mathematical % to the user.
	/// </summary>
	public sealed class ReplicatedPercentageWrapper : IReplicatedConfigContainer {

		private const float ONE_OVER_ONEHUNDRED = 0.01f;

		/// <summary>
		/// The programmable value (from 0 to 1) of the percentage input by the user.
		/// </summary>
		public float Value {
			get => _value.Value * ONE_OVER_ONEHUNDRED;
			set => _value.LocalValue = value * 100;
		}

		/// <summary>
		/// The raw value (from 0 to 100) of the percentage input by the user.
		/// </summary>
		public float RawValue100 {
			get => _value.Value;
			set => _value.LocalValue = value; // thats a lot of value
		}

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _value;

		/// <summary>
		/// Construct a new config entry that shows as a percentage (value between 0 and 100) to the user, but as a decimal (value between 0 and 1) to the programmer.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		public ReplicatedPercentageWrapper(AdvancedConfigBuilder parent, string name, string description, float @default, float min = 0, float max = 100, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired, string formatString = "{0}%") {
			_value = (ReplicatedConfigEntry<float>)parent.BindFloatInternal(name, description, @default, min, max, 1f, restartRequired, formatString, CommonPredicates.ONLY_ENABLE_AS_HOST, true, true);
			_value.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
		}

		/// <summary>
		/// Returns true if the provided <see cref="ConfigEntryBase"/> is one of the internal backing configs for this object.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool BelongsToThis(ConfigEntryBase toCheck) {
			return toCheck == _value.BaseLocalBackingConfig;
		}

		/// <summary>
		/// Fires when the value changes. This value is from 0 to 1.
		/// </summary>
		public event Action<float> SettingChanged;

	}

	/// <summary>
	/// This class wraps a floating point config entry ranging from 0 to 100 such that it functions like a percentage in code, but a mathematical % to the user.
	/// </summary>
	public sealed class LocalPercentageWrapper : IReplicatedConfigContainer {

		private const float ONE_OVER_ONEHUNDRED = 0.01f;

		/// <summary>
		/// The programmable value (from 0 to 1) of the percentage input by the user.
		/// </summary>
		public float Value {
			get => _value.Value * ONE_OVER_ONEHUNDRED;
			set => _value.Value = value * 100;
		}

		/// <summary>
		/// The raw value (from 0 to 100) of the percentage input by the user.
		/// </summary>
		public float RawValue100 {
			get => _value.Value;
			set => _value.Value = value; // thats a lot of value
		}

		private readonly ConfigEntry<float> _value;

		/// <summary>
		/// Construct a new config entry that shows as a percentage (value between 0 and 100) to the user, but as a decimal (value between 0 and 1) to the programmer.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		public LocalPercentageWrapper(AdvancedConfigBuilder parent, string name, string description, float @default, float min = 0, float max = 100, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired, string formatString = "{0}%") {
			_value = (ConfigEntry<float>)parent.BindFloatInternal(name, description, @default, min, max, 1f, restartRequired, formatString, null, true, false);
			_value.SettingChanged += (_, _) => SettingChanged?.Invoke(Value);
		}

		/// <summary>
		/// Returns true if the provided <see cref="ConfigEntryBase"/> is one of the internal backing configs for this object.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool BelongsToThis(ConfigEntryBase toCheck) {
			return toCheck == _value;
		}

		/// <summary>
		/// Fires when the value changes. This value is from 0 to 1.
		/// </summary>
		public event Action<float> SettingChanged;
	}
}
