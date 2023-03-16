using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs;
using static RiskOfOptions.OptionConfigs.BaseOptionConfig;

namespace Xan.ROR2VoidPlayerCharacterCommon.ROOInterop {

	/// <summary>
	/// This class manages all configuration options to include them in Risk of Options by default.
	/// </summary>
	public sealed class AdvancedConfigBuilder {

		private readonly List<BasicConfigData> _configs = new List<BasicConfigData>();

		private readonly ConfigFile _cfg;

		private string _currentCategory = "Uncategorized";

		private readonly string _modId;
		private readonly string _modName;

		/// <summary>
		/// Create a new wrapper using the provided <see cref="ConfigFile"/>.
		/// </summary>
		/// <param name="cfg"></param>
		/// <param name="icon">An icon to use for the mod. May be null.</param>
		/// <param name="modId"></param>
		/// <param name="modName">The name of this mod as displayed in Risk of Options.</param>
		/// <param name="modDesc"></param>
		public AdvancedConfigBuilder(ConfigFile cfg, Sprite icon, string modId, string modName, string modDesc) {
			_cfg = cfg;
			ModSettingsManager.SetModDescription(modDesc, modId, modName);
			if (icon != null) ModSettingsManager.SetModIcon(icon);
			_modId = modId;
			_modName = modName;
		}

		/// <summary>
		/// Changes the category that config options are sent into.
		/// </summary>
		/// <param name="category"></param>
		public void SetCategory(string category) {
			_currentCategory = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category;
		}

		/// <summary>
		/// Binds a floating point value as a config option.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="increment"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString">A format string to use when displaying the value.</param>
		/// <param name="isDisabledDelegate">A delegate to enable or disable the option.</param>
		/// <returns></returns>
		public ConfigEntry<float> Bind(string name, float @default, string description, float min = 0f, float max = float.MaxValue, float? increment = null, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = null, IsDisabledDelegate isDisabledDelegate = null) {
			return Bind(name, @default, description, min, max, increment, restartRequired, formatString, isDisabledDelegate, false);
		}

		internal ConfigEntry<float> Bind(string name, float @default, string description, float min, float max, float? increment, RestartType restartRequired, string formatString, IsDisabledDelegate isDisabledDelegate, bool noDoc) {
			ConfigEntry<float> entry = _cfg.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description), new AcceptableValueRange<float>(min, max)));
			if (!increment.HasValue) {
				SliderOption slider = new SliderOption(entry, new SliderConfig {
					name = name,
					category = _currentCategory,
					description = description + RestartTypeToString(restartRequired, false),
					min = min,
					max = max,
					formatString = formatString,
					checkIfDisabled = isDisabledDelegate,
					restartRequired = restartRequired == RestartType.RestartGame
				});
				ModSettingsManager.AddOption(slider, _modId, _modName);
			} else {
				StepSliderOption slider = new StepSliderOption(entry, new StepSliderConfig {
					name = name,
					category = _currentCategory,
					description = description + RestartTypeToString(restartRequired, false),
					min = min,
					max = max,
					increment = increment.Value,
					formatString = formatString,
					checkIfDisabled = isDisabledDelegate,
					restartRequired = restartRequired == RestartType.RestartGame
				});
				ModSettingsManager.AddOption(slider, _modId, _modName);
			}
			if (!noDoc) {
				_configs.Add(new BasicConfigData {
					type = "Decimal",
					name = name,
					category = _currentCategory,
					description = description + RestartTypeToString(restartRequired, false),
					restartRequired = restartRequired
				});
			}
			return entry;
		}

		/// <summary>
		/// Binds an integer value as a config option.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString">A format string to use when displaying the value.</param>
		/// <param name="isDisabledDelegate">A delegate to enable or disable the option.</param>
		/// <returns></returns>
		public ConfigEntry<int> Bind(string name, int @default, string description, int min = 0, int max = int.MaxValue, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = null, IsDisabledDelegate isDisabledDelegate = null) {
			ConfigEntry<int> entry = _cfg.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description), new AcceptableValueRange<int>(min, max)));
			IntSliderOption slider = new IntSliderOption(entry, new IntSliderConfig {
				name = name,
				category = _currentCategory,
				description = description,
				min = min,
				max = max,
				formatString = formatString,
				checkIfDisabled = isDisabledDelegate,
				restartRequired = restartRequired == RestartType.RestartGame
			});
			ModSettingsManager.AddOption(slider, _modId, _modName);

			_configs.Add(new BasicConfigData {
				type = "Whole Number",
				name = name,
				category = _currentCategory,
				description = description + RestartTypeToString(restartRequired, false),
				restartRequired = restartRequired
			});
			return entry;
		}

		/// <summary>
		/// Binds an enum as a dropdown option.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="restartRequired"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <returns></returns>
		public ConfigEntry<TEnum> Bind<TEnum>(string name, TEnum @default, string description, RestartType restartRequired = RestartType.NoRestartRequired, IsDisabledDelegate isDisabledDelegate = null) where TEnum : Enum {
			ConfigEntry<TEnum> entry = _cfg.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description)));
			ChoiceOption choice = new ChoiceOption(entry, new ChoiceConfig {
				name = name,
				category = _currentCategory,
				description = description,
				checkIfDisabled = isDisabledDelegate,
				restartRequired = restartRequired == RestartType.RestartGame
			});
			ModSettingsManager.AddOption(choice, _modId, _modName);
			_configs.Add(new BasicConfigData {
				type = typeof(TEnum).Name,
				name = name,
				category = _currentCategory,
				description = description + RestartTypeToString(restartRequired, false),
				restartRequired = restartRequired
			});
			return entry;
		}

		/// <summary>
		/// Binds a boolean option.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="restartRequired"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <returns></returns>
		public ConfigEntry<bool> Bind(string name, bool @default, string description, RestartType restartRequired = RestartType.NoRestartRequired, IsDisabledDelegate isDisabledDelegate = null) {
			ConfigEntry<bool> entry = _cfg.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description)));
			CheckBoxOption option = new CheckBoxOption(entry, new CheckBoxConfig {
				name = name,
				category = _currentCategory,
				description = description,
				checkIfDisabled = isDisabledDelegate,
				restartRequired = restartRequired == RestartType.RestartGame
			});
			ModSettingsManager.AddOption(option, _modId, _modName);
			_configs.Add(new BasicConfigData {
				type = "Toggle (true/false)",
				name = name,
				category = _currentCategory,
				description = description + RestartTypeToString(restartRequired, false),
				restartRequired = restartRequired
			});
			return entry;
		}

		/// <summary>
		/// Binds a <see cref="Vector3"/> as a config option, albeit as three distinct floating point options rather than a json Vector3 object like traditional BepInEx configs do.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <returns></returns>
		public Vector3Wrapper Bind(string name, Vector3 @default, string description, Vector3 min = default, Vector3? max = null, RestartType restartRequired = RestartType.NoRestartRequired) {
			Vector3Wrapper wrapper = new Vector3Wrapper(this, name, @default, StripTags(description), min, max, restartRequired);
			_configs.Add(new BasicConfigData {
				type = "3D Coordinate",
				name = name,
				category = _currentCategory,
				description = description + RestartTypeToString(restartRequired, false),
				restartRequired = restartRequired
			});
			return wrapper;
		}

		/// <summary>
		/// Binds a floating point value as a percentage displayed as 0-100 to the user, but available as 0-1 to the programmer.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <returns></returns>
		public PercentageWrapper BindFloatPercentage(string name, float @default, string description, float min = 0f, float max = 100f, RestartType restartRequired = RestartType.NoRestartRequired) {
			PercentageWrapper wrapper = new PercentageWrapper(this, name, @default, StripTags(description), min, max, restartRequired);
			_configs.Add(new BasicConfigData {
				type = "Percentage",
				name = name,
				category = _currentCategory,
				description = description + RestartTypeToString(restartRequired, false),
				restartRequired = restartRequired
			});
			return wrapper;
		}

		/// <summary>
		/// Exports the current config system as a markdown table.
		/// </summary>
		/// <returns></returns>
		public string ToMarkdownTable() {
			StringBuilder md = new StringBuilder("# Configurable Stats\n");
			Dictionary<string, List<BasicConfigData>> categories = new Dictionary<string, List<BasicConfigData>>();
			foreach (BasicConfigData cfg in _configs) {
				if (!categories.TryGetValue(cfg.category, out List<BasicConfigData> orderedEntries)) {
					orderedEntries = new List<BasicConfigData>();
					categories[cfg.category] = orderedEntries;
				}
				orderedEntries.Add(cfg);
			}

			foreach (KeyValuePair<string, List<BasicConfigData>> entries in categories) {
				md.AppendLine($"## {entries.Key}\n");
				md.AppendLine("| Name | Type | Description | Applies... | ");
				md.AppendLine("|---|---|---|---|");
				foreach (BasicConfigData cfg in entries.Value) {
					md.Append("| ");
					md.Append(Sanitize(StripTags(cfg.name)));
					md.Append(" | ");
					md.Append(Sanitize(StripTags(cfg.type)));
					md.Append(" | ");
					md.Append(Sanitize(StripTags(cfg.description)));
					md.Append(" | ");
					md.Append(RestartTypeToString(cfg.restartRequired, true));
					md.AppendLine(" |");
				}
				md.AppendLine("\n");
			}

			return md.ToString();
		}


		private readonly Regex _stripTags = new Regex(@"(<[\w\d=/]*>)");
		private string StripTags(string @in) => _stripTags.Replace(@in, "");

		private string Sanitize(string input) {
			return input.Replace("\\n", "<br/>").Replace("\n", "<br/>").Replace("\\", "\\\\").Replace("|", "\\|");
		}

		private string RestartTypeToString(RestartType type, bool isForMarkdown) {
			if (type == RestartType.NoRestartRequired) {
				return isForMarkdown ? "Immediately" : "\n\n<style=cIsHealing>This setting applies immediately.</style>";
			} else if (type == RestartType.NextRespawn) {
				return isForMarkdown ? "On the next Run, Stage, or Respawn (if available)" : "\n\n<style=cIsDamage>This setting applies the next time the affected character(s) spawn in (including respawns).</style>";
			} else {
				return isForMarkdown ? "After restarting the game" : "\n\n<style=cDeath>This setting requires a restart to apply.</style>";
			}
		}

		private class BasicConfigData {

			public string type;
			public string name;
			public string category;
			public string description;
			public RestartType restartRequired;

		}

		/// <summary>
		/// Sometimes the game or run needs to be restarted for a setting to work. This represents that limit.
		/// </summary>
		public enum RestartType {
			
			/// <summary>
			/// This setting can be freely changed in game or in the menu without requiring anything to restart. It just works(tm)
			/// </summary>
			NoRestartRequired,

			/// <summary>
			/// This setting can be changed without restarting the entire game, but it won't apply until the next time the affected character spawns.
			/// </summary>
			NextRespawn,

			/// <summary>
			/// This setting needs the entire game to be restarted.
			/// </summary>
			RestartGame

		}
	}
}
