using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {

	/// <summary>
	/// This class wraps a floating point config entry ranging from 0 to 100 such that it functions like a percentage in code, but a mathematical % to the user.
	/// </summary>
	public sealed class PercentageWrapper {

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

		private ConfigEntry<float> _value;

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
		public PercentageWrapper(AdvancedConfigBuilder parent, string name, float @default, string description, float min = 0, float max = 100, AdvancedConfigBuilder.RestartType restartRequired = AdvancedConfigBuilder.RestartType.NoRestartRequired) {
			_value = parent.Bind(name, @default, description, min, max, null, restartRequired, "{0}%", null, true);
		}

	}
}
