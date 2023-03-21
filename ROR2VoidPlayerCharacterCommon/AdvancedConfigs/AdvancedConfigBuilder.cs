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
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using static RiskOfOptions.OptionConfigs.BaseOptionConfig;

namespace Xan.ROR2VoidPlayerCharacterCommon.ROOInterop {

	/// <summary>
	/// This class manages all configuration options to include them in Risk of Options by default.
	/// </summary>
	public sealed class AdvancedConfigBuilder {

		private readonly List<BasicConfigData> _configs = new List<BasicConfigData>();

		/// <summary>
		/// The <see cref="ConfigFile"/> that this is built on top of.
		/// </summary>
		public ConfigFile BackingConfig { get; private set; }

		private string _currentCategory = "Uncategorized";

		private readonly string _modId;
		private readonly string _modName;
		private readonly Type _configClass;

		/// <summary>
		/// This class is used to replicate and manage the config values used by clients when they are connected to this client.
		/// </summary>
		public ConfigurationReplicator Replicator { get; private set; }

		/// <summary>
		/// A common string used to tell people that the data is replicated.
		/// </summary>
		public const string REPLICATED_NOTICE = "<style=cIsUtility>This value is managed by the host of the session.</style> In multiplayer environments, you will not be able to change this setting, and will always use what the host has it set to.\n\n";
		

		/// <summary>
		/// Create a new wrapper using the provided <see cref="ConfigFile"/>. Remember to use <see cref="CreateConfigAutoReplicator"/> after all configs have been registered.
		/// </summary>
		/// <param name="classContainingConfigs">This should be the type of whatever class contains the config entries. The entries should be decorated with <see cref="ReplicatedConfigurationAttribute"/>.</param>
		/// <param name="cfg"></param>
		/// <param name="icon">An icon to use for the mod. May be null.</param>
		/// <param name="modId"></param>
		/// <param name="modName">The name of this mod as displayed in Risk of Options.</param>
		/// <param name="modDesc"></param>
		public AdvancedConfigBuilder(Type classContainingConfigs, ConfigFile cfg, Sprite icon, string modId, string modName, string modDesc) {
			BackingConfig = cfg;
			ModSettingsManager.SetModDescription(modDesc, modId, modName);
			if (icon != null) ModSettingsManager.SetModIcon(icon, modId, modName);
			_modId = modId;
			_modName = modName;
			_configClass = classContainingConfigs;
		}

		/// <summary>
		/// Sets up this configuration object for automatic replication. This should be done <strong>after</strong> all config-containing fields have been set.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public void CreateConfigAutoReplicator() {
			if (Replicator != null) throw new InvalidOperationException("The replicator has already been set up.");
			Replicator = ConfigurationReplicator.CreateReplicator(_modId, _configClass);
			BackingConfig.SettingChanged += OnSettingChanged;
		}


		private void OnSettingChanged(object sender, SettingChangedEventArgs e) {
			Replicator.TrySendChanges(e);
		}

		/// <summary>
		/// Changes the category that config options are sent into.
		/// </summary>
		/// <param name="category"></param>
		public void SetCategory(string category) {
			_currentCategory = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category;
		}

		private string GetDescription(string inputDescription, RestartType restartType, bool replicated, bool isForMarkdown = false) {
			return (replicated ? REPLICATED_NOTICE : string.Empty) + inputDescription + RestartTypeToString(restartType, isForMarkdown);
		}

		/// <summary>
		/// Binds a floating point value as a config option. This config option is sent across the network and synced to the host.
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
		public ReplicatedConfigEntry<float> BindReplicated(string name, string description, float @default, float min = 0f, float max = float.MaxValue, float? increment = null, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = null, IsDisabledDelegate isDisabledDelegate = null) {
			return (ReplicatedConfigEntry<float>)BindFloatInternal(name, description, @default, min, max, increment, restartRequired, formatString, isDisabledDelegate, false, true);
		}

		/// <summary>
		/// Binds a floating point value as a config option. This config option is local and does not get synced with the host.
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
		public ConfigEntry<float> BindLocal(string name, string description, float @default, float min = 0f, float max = float.MaxValue, float? increment = null, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = null, IsDisabledDelegate isDisabledDelegate = null) {
			return (ConfigEntry<float>)BindFloatInternal(name, description, @default, min, max, increment, restartRequired, formatString, isDisabledDelegate, false, false);
		}

		/// <summary>
		/// Agnostic method for the float variants of BindReplicated and BindLocal since those are used by the wrappers. This returns either <see cref="ReplicatedConfigEntry{T}"/> or <see cref="ConfigEntry{T}"/> depending on the <paramref name="replicated"/> parameter.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="increment"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <param name="noDoc"></param>
		/// <param name="replicated"></param>
		/// <returns></returns>
		internal object BindFloatInternal(string name, string description, float @default, float min, float max, float? increment, RestartType restartRequired, string formatString, IsDisabledDelegate isDisabledDelegate, bool noDoc, bool replicated) {
			ConfigEntry<float> entry = BackingConfig.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description), new AcceptableValueRange<float>(min, max)));
			if (!increment.HasValue) {
				SliderOption slider = new SliderOption(entry, new SliderConfig {
					name = name,
					category = _currentCategory,
					description = GetDescription(description, restartRequired, replicated),
					min = min,
					max = max,
					formatString = formatString ?? "{0}",
					checkIfDisabled = isDisabledDelegate,
					restartRequired = restartRequired == RestartType.RestartGame
				});
				ModSettingsManager.AddOption(slider, _modId, _modName);
			} else {
				StepSliderOption slider = new StepSliderOption(entry, new StepSliderConfig {
					name = name,
					category = _currentCategory,
					description = GetDescription(description, restartRequired, replicated),
					min = min,
					max = max,
					increment = increment.Value,
					formatString = formatString ?? "{0}",
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
					description = GetDescription(description, restartRequired, replicated, true),
					restartRequired = restartRequired
				});
			}
			if (replicated) {
				return new ReplicatedConfigEntry<float>(entry);
			} else {
				return entry;
			}
		}

		/// <summary>
		/// Binds an integer value as a config option. This value is sent across the network and synced to the host.
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
		public ReplicatedConfigEntry<int> BindReplicated(string name, string description, int @default, int min = 0, int max = int.MaxValue, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = null, IsDisabledDelegate isDisabledDelegate = null) {
			return new ReplicatedConfigEntry<int>(CommonBindInt(name, description, @default, min, max, restartRequired, formatString, isDisabledDelegate, true));
		}

		/// <summary>
		/// Binds an integer value as a config option. This value is only on the local machine and does not get synced over the network.
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
		public ConfigEntry<int> BindLocal(string name, string description, int @default, int min = 0, int max = int.MaxValue, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = null, IsDisabledDelegate isDisabledDelegate = null) {
			return CommonBindInt(name, description, @default, min, max, restartRequired, formatString, isDisabledDelegate, false);
		}

		private ConfigEntry<int> CommonBindInt(string name, string description, int @default, int min, int max, RestartType restartRequired, string formatString, IsDisabledDelegate isDisabledDelegate, bool replicated) {
			ConfigEntry<int> entry = BackingConfig.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description), new AcceptableValueRange<int>(min, max)));
			IntSliderOption slider = new IntSliderOption(entry, new IntSliderConfig {
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, replicated),
				min = min,
				max = max,
				formatString = formatString ?? "{0}",
				checkIfDisabled = isDisabledDelegate,
				restartRequired = restartRequired == RestartType.RestartGame
			});
			ModSettingsManager.AddOption(slider, _modId, _modName);

			_configs.Add(new BasicConfigData {
				type = "Whole Number",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, replicated, true),
				restartRequired = restartRequired
			});
			return entry;
		}

		/// <summary>
		/// Binds an enum as a dropdown option. This option is sent across the network and synced to the host.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="restartRequired"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <returns></returns>
		public ReplicatedConfigEntry<TEnum> BindReplicated<TEnum>(string name, string description, TEnum @default, RestartType restartRequired = RestartType.NoRestartRequired, IsDisabledDelegate isDisabledDelegate = null) where TEnum : Enum {
			return new ReplicatedConfigEntry<TEnum>(CommonBindEnum(name, description, @default, restartRequired, isDisabledDelegate, true));
		}

		/// <summary>
		/// Binds an enum as a dropdown option. This option is stored locally and is not synced across the network.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="restartRequired"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <returns></returns>
		public ConfigEntry<TEnum> BindLocal<TEnum>(string name, string description, TEnum @default, RestartType restartRequired = RestartType.NoRestartRequired, IsDisabledDelegate isDisabledDelegate = null) where TEnum : Enum {
			return CommonBindEnum(name, description, @default, restartRequired, isDisabledDelegate, false);
		}

		private ConfigEntry<TEnum> CommonBindEnum<TEnum>(string name, string description, TEnum @default, RestartType restartRequired, IsDisabledDelegate isDisabledDelegate, bool replicated) where TEnum : Enum {
			ConfigEntry<TEnum> entry = BackingConfig.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description)));
			ChoiceOption choice = new ChoiceOption(entry, new ChoiceConfig {
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, replicated),
				checkIfDisabled = isDisabledDelegate,
				restartRequired = restartRequired == RestartType.RestartGame
			});
			ModSettingsManager.AddOption(choice, _modId, _modName);
			_configs.Add(new BasicConfigData {
				type = typeof(TEnum).Name,
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, replicated, true),
				restartRequired = restartRequired
			});
			return entry;
		}

		/// <summary>
		/// Binds a boolean option. This option is sent across the network and synced with the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="restartRequired"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <returns></returns>
		public ReplicatedConfigEntry<bool> BindReplicated(string name, string description, bool @default, RestartType restartRequired = RestartType.NoRestartRequired, IsDisabledDelegate isDisabledDelegate = null) {
			return new ReplicatedConfigEntry<bool>(CommonBindBool(name, description, @default, restartRequired, isDisabledDelegate, true));
		}

		/// <summary>
		/// Binds a boolean option. This option is kept on the local machine and is not synced with the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="restartRequired"></param>
		/// <param name="isDisabledDelegate"></param>
		/// <returns></returns>
		public ConfigEntry<bool> BindLocal(string name, string description, bool @default, RestartType restartRequired = RestartType.NoRestartRequired, IsDisabledDelegate isDisabledDelegate = null) {
			return CommonBindBool(name, description, @default, restartRequired, isDisabledDelegate, false);
		}

		private ConfigEntry<bool> CommonBindBool(string name, string description, bool @default, RestartType restartRequired, IsDisabledDelegate isDisabledDelegate, bool replicated) {
			ConfigEntry<bool> entry = BackingConfig.Bind(_currentCategory, name, @default, new ConfigDescription(StripTags(description)));
			CheckBoxOption option = new CheckBoxOption(entry, new CheckBoxConfig {
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, replicated),
				checkIfDisabled = isDisabledDelegate,
				restartRequired = restartRequired == RestartType.RestartGame
			});
			ModSettingsManager.AddOption(option, _modId, _modName);
			_configs.Add(new BasicConfigData {
				type = "Toggle (true/false)",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, replicated, true),
				restartRequired = restartRequired
			});
			return entry;
		}

		/// <summary>
		/// Binds a <see cref="Vector3"/> as a config option, albeit as three distinct floating point options rather than a json Vector3 object like traditional BepInEx configs do. This option is sent across the network and synced to the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public ReplicatedVector3Wrapper BindReplicated(string name, string description, Vector3 @default, Vector3 min = default, Vector3? max = null, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = "{0}m") {
			ReplicatedVector3Wrapper wrapper = new ReplicatedVector3Wrapper(this, name, StripTags(description), @default, min, max, restartRequired, formatString);
			_configs.Add(new BasicConfigData {
				type = "3D Coordinate",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, true, true),
				restartRequired = restartRequired
			});
			return wrapper;
		}

		/// <summary>
		/// Binds a <see cref="Vector3"/> as a config option, albeit as three distinct floating point options rather than a json Vector3 object like traditional BepInEx configs do. This option is local and does not sync with the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public LocalVector3Wrapper BindLocal(string name, string description, Vector3 @default, Vector3 min = default, Vector3? max = null, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = "{0}m") {
			LocalVector3Wrapper wrapper = new LocalVector3Wrapper(this, name, StripTags(description), @default, min, max, restartRequired, formatString);
			_configs.Add(new BasicConfigData {
				type = "3D Coordinate",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, false, true),
				restartRequired = restartRequired
			});
			return wrapper;
		}

		/// <summary>
		/// Binds a floating point value as a percentage displayed as 0-100 to the user, but available as 0-1 to the programmer. This value is sent across the network and synced to the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default">The default value, in the range of 0 to 100 (not 0 to 1!)</param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public ReplicatedPercentageWrapper BindFloatPercentageReplicated(string name, string description, float @default, float min = 0f, float max = 100f, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = "{0}%") {
			ReplicatedPercentageWrapper wrapper = new ReplicatedPercentageWrapper(this, name, description, @default, min, max, restartRequired, formatString);
			_configs.Add(new BasicConfigData {
				type = "Percentage",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, true, true),
				restartRequired = restartRequired
			});
			return wrapper;
		}


		/// <summary>
		/// Binds a floating point value as a percentage displayed as 0-100 to the user, but available as 0-1 to the programmer. This value is stored locally and does not sync with the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="default">The default value, in the range of 0 to 100 (not 0 to 1!)</param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public LocalPercentageWrapper BindFloatPercentageLocal(string name, string description, float @default, float min = 0f, float max = 100f, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = "{0}%") {
			LocalPercentageWrapper wrapper = new LocalPercentageWrapper(this, name, description, @default, min, max, restartRequired, formatString);
			_configs.Add(new BasicConfigData {
				type = "Percentage",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, false, true),
				restartRequired = restartRequired
			});
			return wrapper;
		}

		/// <summary>
		/// Constructs a wrapper storing a range as (min, max). Typically a <see cref="Vector2"/> would be used here but it is not appropriate here. This value is sent across the network and synced to the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultMin">The default value for the minimum value in the range.</param>
		/// <param name="defaultMax">The default value for the maximum value in the range.</param>
		/// <param name="description"></param>
		/// <param name="min">Neither <paramref name="defaultMin"/> nor <paramref name="defaultMax"/> is allowed to be set lower than this value.</param>
		/// <param name="max">Neither <paramref name="defaultMin"/> nor <paramref name="defaultMax"/> is allowed to be set higher than this value.</param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public ReplicatedMinMaxWrapper BindMinMaxReplicated(string name, string description, float defaultMin, float defaultMax, float min = 0f, float max = 1f, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = "{0}") {
			ReplicatedMinMaxWrapper wrapper = new ReplicatedMinMaxWrapper(this, name, description, defaultMin, defaultMax, min, max, restartRequired, formatString);
			_configs.Add(new BasicConfigData {
				type = "[Min, Max] Range",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, true, true),
				restartRequired = restartRequired
			});
			return wrapper;
		}

		/// <summary>
		/// Constructs a wrapper storing a range as (min, max). Typically a <see cref="Vector2"/> would be used here but it is not appropriate here. This value is stored locally and does not sync with the host.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultMin">The default value for the minimum value in the range.</param>
		/// <param name="defaultMax">The default value for the maximum value in the range.</param>
		/// <param name="description"></param>
		/// <param name="min">Neither <paramref name="defaultMin"/> nor <paramref name="defaultMax"/> is allowed to be set lower than this value.</param>
		/// <param name="max">Neither <paramref name="defaultMin"/> nor <paramref name="defaultMax"/> is allowed to be set higher than this value.</param>
		/// <param name="restartRequired"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public LocalMinMaxWrapper BindMinMaxLocal(string name, string description, float defaultMin, float defaultMax, float min = 0f, float max = 1f, RestartType restartRequired = RestartType.NoRestartRequired, string formatString = "{0}") {
			LocalMinMaxWrapper wrapper = new LocalMinMaxWrapper(this, name, description, defaultMin, defaultMax, min, max, restartRequired, formatString);
			_configs.Add(new BasicConfigData {
				type = "[Min, Max] Range",
				name = name,
				category = _currentCategory,
				description = GetDescription(description, restartRequired, false, true),
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
				return isForMarkdown ? "On the next Run, Stage, or Respawn (if available)" : "\n\n<style=cIsDamage>This setting applies the next time the affected character (re)spawns.</style>";
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
