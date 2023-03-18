using BepInEx.Configuration;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;
using Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {

	/// <summary>
	/// Some configuration values, such as base stats, are common across all void characters. Construct an instance of this class to automatically register and create all of these options.
	/// <para/>
	/// Decorate this class with <see cref="ReplicatedConfigurationAttribute"/> when you create an instance for your mod.
	/// </summary>
	public class CommonModAndCharacterConfigs : IReplicatedConfigContainer {

		#region Base Stats
		/// <summary>
		/// The character's starting maximum health.
		/// </summary>
		public float BaseMaxHealth {
			get => _baseMaxHealth.Value;
			set => _baseMaxHealth.LocalValue = value;
		}

		/// <summary>
		/// Maximum health that the character earns every time they level up.
		/// </summary>
		public float LevelMaxHealth {
			get => _levelMaxHealth.Value;
			set => _levelMaxHealth.LocalValue = value;
		}

		/// <summary>
		/// The character's starting walk speed.
		/// </summary>
		public float BaseMoveSpeed {
			get => _baseMoveSpeed.Value;
			set => _baseMoveSpeed.LocalValue = value;
		}

		/// <summary>
		/// The additional walk speed the character earns every time they level up.
		/// </summary>
		public float LevelMoveSpeed {
			get => _levelMoveSpeed.Value;
			set => _levelMoveSpeed.LocalValue = value;
		}

		/// <summary>
		/// The walk speed is multiplied by this value to compute the sprint speed.
		/// </summary>
		public float SprintSpeedMultiplier {
			get => _sprintSpeedMultiplier.Value;
			set => _sprintSpeedMultiplier.LocalValue = value;
		}

		/// <summary>
		/// The amount of health regenerated per second by default.
		/// </summary>
		public float BaseHPRegen {
			get => _baseHPRegen.Value;
			set => _baseHPRegen.LocalValue = value;
		}

		/// <summary>
		/// For every level the character earns, the health regeneration rate will go up by this many HP per second.
		/// </summary>
		public float LevelHPRegen {
			get => _levelHPRegen.Value;
			set => _levelHPRegen.LocalValue = value;
		}

		/// <summary>
		/// The armor that this character starts with.
		/// </summary>
		public float BaseArmor {
			get => _baseArmor.Value;
			set => _baseArmor.LocalValue = value;
		}

		/// <summary>
		/// The armor that is added for every level this character earns.
		/// </summary>
		public float LevelArmor {
			get => _levelArmor.Value;
			set => _levelArmor.LocalValue = value;
		}

		/// <summary>
		/// The base damage. All attacks use this value to compute their resulting damage.
		/// </summary>
		public float BaseDamage {
			get => _baseDamage.Value;
			set => _baseDamage.LocalValue = value;
		}

		/// <summary>
		/// Every time the character levels up, the base damage increases by this much.
		/// </summary>
		public float LevelDamage {
			get => _levelDamage.Value;
			set => _levelDamage.LocalValue = value;
		}

		/// <summary>
		/// The default chance (from 0 to 100) that this character will land a critical hit.
		/// </summary>
		public float BaseCritChance {
			get => _baseCritChance.Value;
			set => _baseCritChance.LocalValue = value;
		}

		/// <summary>
		/// Every time the character levels up, the critical hit chance increases by this much.
		/// </summary>
		public float LevelCritChance {
			get => _levelCritChance.Value;
			set => _levelCritChance.LocalValue = value;
		}

		/// <summary>
		/// The default maximum shield that this character has.
		/// </summary>
		public float BaseMaxShield {
			get => _baseMaxShield.Value;
			set => _baseMaxShield.LocalValue = value;
		}

		/// <summary>
		/// Every time this character levels up, the total shield it has gets this added to it.
		/// </summary>
		public float LevelMaxShield {
			get => _levelMaxShield.Value;
			set => _levelMaxShield.LocalValue = value;
		}

		/// <summary>
		/// The acceleration of the character determines how quickly it reaches its move speed. Lower values make the world feel slippery.
		/// </summary>
		public float BaseAcceleration {
			get => _baseAcceleration.Value;
			set => _baseAcceleration.LocalValue = value;
		}

		/// <summary>
		/// The amount of jumps this character has, 0 disabling jumping entirely.
		/// </summary>
		public int BaseJumpCount {
			get => _baseJumpCount.Value;
			set => _baseJumpCount.LocalValue = value;
		}

		/// <summary>
		/// The amount of upward force applied when jumping.
		/// </summary>
		public float BaseJumpPower {
			get => _baseJumpPower.Value;
			set => _baseJumpPower.LocalValue = value;
		}

		/// <summary>
		/// Every time the character levels up, the upward jump force is increased by this much.
		/// </summary>
		public float LevelJumpPower {
			get => _levelJumpPower.Value;
			set => _levelJumpPower.LocalValue = value;
		}

		/// <summary>
		/// The starting attack speed that this character has.
		/// </summary>
		public float BaseAttackSpeed {
			get => _baseAttackSpeed.Value;
			set => _baseAttackSpeed.LocalValue = value;
		}

		/// <summary>
		/// Every time this character levels up, the attack speed increases by this much.
		/// </summary>
		public float LevelAttackSpeed {
			get => _levelAttackSpeed.Value;
			set => _levelAttackSpeed.LocalValue = value;
		}

		/// <summary>
		/// If true, do <em>not</em> downscale the character.
		/// </summary>
		public bool UseFullSizeCharacter {
			get => _useFullSizeCharacter.Value;
			set => _useFullSizeCharacter.LocalValue = value;
		}

		/// <summary>
		/// Enable trace logging. Detailed logs. You get it. I'm watching youtube I don't wanna write docs anymore.
		/// </summary>
		public bool TraceLogging {
			get => _traceLogging.Value;
			set => _traceLogging.Value = value;
		}

		/// <summary>
		/// The transparency of the character model whilst in combat, as a value between 0 and 100.
		/// </summary>
		public int TransparencyInCombat {
			get => _transparencyInCombat.Value;
			set => _transparencyInCombat.Value = value;
		}

		/// <summary>
		/// The transparency of the character model whilst out of combat, as a value between 0 and 100.
		/// </summary>
		public int TransparencyOutOfCombat {
			get => _transparencyOutOfCombat.Value;
			set => _transparencyOutOfCombat.Value = value;
		}

		/// <summary>
		/// The vertical pivot offset of the camera.
		/// </summary>
		public float CameraPivotOffset {
			get => _cameraPivotOffset.Value;
			set => _cameraPivotOffset.Value = value;
		}

		/// <summary>
		/// The offset of the camera from the character.
		/// </summary>
		public Vector3 CameraOffset {
			get => _cameraOffset.Value;
			set => _cameraOffset.Value = value;
		}

		#endregion

		#region Backing
		#region Base Stats
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseMaxHealth;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelMaxHealth;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseMoveSpeed;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelMoveSpeed;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _sprintSpeedMultiplier;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseHPRegen;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelHPRegen;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseArmor;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelArmor;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseDamage;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelDamage;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseCritChance;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelCritChance;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseMaxShield;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelMaxShield;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseAcceleration;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<int> _baseJumpCount;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseJumpPower;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelJumpPower;

		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _baseAttackSpeed;
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<float> _levelAttackSpeed;

		#endregion

		#region Misc.
		[ReplicatedConfiguration]
		private readonly ReplicatedConfigEntry<bool> _useFullSizeCharacter;
		
		private readonly ConfigEntry<bool> _traceLogging;
		private readonly ConfigEntry<int> _transparencyInCombat;
		private readonly ConfigEntry<int> _transparencyOutOfCombat;
		private readonly ConfigEntry<float> _cameraPivotOffset;
		private readonly LocalVector3Wrapper _cameraOffset;
		#endregion
		#endregion

		//private readonly ConfigEntryBase[] _cfgs;

		private const string FMT_DEFAULT = "The base {0} that the character has on a new run.";
		private const string FMT_LEVELED = "For each level the player earns, the base {0} increases by this amount.";
		private const string FMT_TRANSPARENCY = "The transparency of the character when you are {0}.\n\nThis can be used to make it easier to see enemies by making your body transparent to prevent it from getting in the way.\n\nA value of 0 means fully opaque, and a value of 100 means as invisible as possible.";

		/// <summary>
		/// Automatically creates two configs for a base and leveled stat of the same type.
		/// </summary>
		/// <param name="aCfg"></param>
		/// <param name="statName"></param>
		/// <param name="defaultBase"></param>
		/// <param name="defaultLeveled"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="base"></param>
		/// <param name="leveled"></param>
		public static void MakeBaseAndLeveled(AdvancedConfigBuilder aCfg, string statName, float defaultBase, float defaultLeveled, float min, float max, ref ReplicatedConfigEntry<float> @base, ref ReplicatedConfigEntry<float> leveled) {
			@base = aCfg.BindReplicated($"Base {statName}", string.Format(FMT_DEFAULT, statName), defaultBase, min, max, 0.5f, restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
			leveled = aCfg.BindReplicated($"Leveled {statName}", string.Format(FMT_LEVELED, statName), defaultLeveled, min, max, 0.5f, restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
		}

		/// <summary>
		/// Using the provided <see cref="AdvancedConfigBuilder"/>, this will register all underlying <see cref="CharacterBody"/> stats as config entries.
		/// </summary>
		/// <param name="aCfg"></param>
		/// <param name="defaults">Stores the default values for every option.</param>
		public CommonModAndCharacterConfigs(AdvancedConfigBuilder aCfg, Defaults defaults) {
			aCfg.SetCategory("Mod Meta, Graphics, Gameplay");
			_traceLogging = aCfg.BindLocal("Trace Logging", "Trace Logging is a practice where individual steps through the code are logged. This can dramatically increase the size and clutter within your log file, and should only really be used when debugging.", defaults.TraceLogging);
			_transparencyInCombat = aCfg.BindLocal("Transparency In Combat", string.Format(FMT_TRANSPARENCY, "in combat"), defaults.TransparencyInCombat, 0, 100);
			_transparencyOutOfCombat = aCfg.BindLocal("Transparency Out Of Combat", string.Format(FMT_TRANSPARENCY, "not in combat"), defaults.TransparencyOutOfCombat, 0, 100);
			_useFullSizeCharacter = aCfg.BindReplicated("Use Full Size Character", "By default, the player character has its size reduced to 50% its normal scale. Enabling this option will use the full size character model.\n\n<style=cDeath>Warning:</style> Using the full size character is known to cause some problems, especially in that it makes certain areas of the map impossible to access.", defaults.UseFullSizeCharacter, AdvancedConfigBuilder.RestartType.NextRespawn);

			aCfg.SetCategory("Camera");
			_cameraPivotOffset = aCfg.BindLocal("Camera Pivot Offset", "The Camera Pivot Offset is the vertical offset of the <i>pivot point</i> for your camera. If you imagine the camera like it's attached to a ball joint on the character, this is how high up the ball joint is in and of itself.", defaults.CameraPivotOffset, -20, 20, 1);
			_cameraOffset = aCfg.BindLocal("Camera Offset", "In contrast to the pivot offset, the camera offset is the 3D offset of the camera itself. This offset applies <i>relative to the camera, after it has rotated</i>.", defaults.CameraOffset, new Vector3(-30, -30, -30), new Vector3(30, 30, 30));

			aCfg.SetCategory("Health, Armor, Shield");
			MakeBaseAndLeveled(aCfg, "Maximum Health", defaults.BaseMaxHealth, defaults.LevelMaxHealth, min: 1f, 2000f, ref _baseMaxHealth, ref _levelMaxHealth);
			MakeBaseAndLeveled(aCfg, "Health Regeneration Rate", defaults.BaseHPRegen, defaults.LevelHPRegen, 0f, 100f, ref _baseHPRegen, ref _levelHPRegen);
			MakeBaseAndLeveled(aCfg, "Armor", defaults.BaseArmor, defaults.LevelArmor, 0f, 500f, ref _baseArmor, ref _levelArmor);
			MakeBaseAndLeveled(aCfg, "Shield", defaults.BaseMaxShield, defaults.LevelMaxShield, 0f, 2000f, ref _baseMaxShield, ref _levelMaxShield);

			aCfg.SetCategory("Mobility and Agility");
			MakeBaseAndLeveled(aCfg, "Walk Speed", defaults.BaseMoveSpeed, defaults.LevelMoveSpeed, 0f, 20f, ref _baseMoveSpeed, ref _levelMoveSpeed);
			MakeBaseAndLeveled(aCfg, "Jump Strength", defaults.BaseJumpPower, defaults.LevelJumpPower, 0f, 100f, ref _baseJumpPower, ref _levelJumpPower);
			_sprintSpeedMultiplier = aCfg.BindReplicated("Sprint Speed Multiplier", "Your sprint speed is equal to your Base Walk Speed times this value.", defaults.SprintSpeedMultiplier, 1f, 10f, 0.5f, restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
			_baseAcceleration = aCfg.BindReplicated("Acceleration", "Acceleration determines how quickly your character gets up to your move speed. Low values make it like walking on ice.", defaults.BaseAcceleration, 0f, 200f, 5f, restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
			_baseJumpCount = aCfg.BindReplicated("Jump Count", "The amount of jumps your character has. 1 is a single jump, 2 is a double jump, etc.", defaults.BaseJumpCount, 0, 10, restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);

			aCfg.SetCategory("Underlying Combat Stats");
			MakeBaseAndLeveled(aCfg, "Damage", defaults.BaseDamage, defaults.LevelDamage, 0f, 100f, ref _baseDamage, ref _levelDamage);
			MakeBaseAndLeveled(aCfg, "Crit Chance", defaults.BaseCritChance, defaults.LevelCritChance, 0f, 100f, ref _baseCritChance, ref _levelCritChance);
			MakeBaseAndLeveled(aCfg, "Attack Speed", defaults.BaseAttackSpeed, defaults.LevelAttackSpeed, 0f, 10f, ref _baseAttackSpeed, ref _levelAttackSpeed);

			_cameraPivotOffset.SettingChanged += CameraSettingChangedNative;
			_cameraOffset.SettingChanged += CameraSettingChanged;

			_baseMaxHealth.SettingChanged += StatSettingChangedF;
			_levelMaxHealth.SettingChanged += StatSettingChangedF;
			_baseHPRegen.SettingChanged += StatSettingChangedF;
			_levelHPRegen.SettingChanged += StatSettingChangedF;
			_baseArmor.SettingChanged += StatSettingChangedF;
			_levelArmor.SettingChanged += StatSettingChangedF;
			_baseMaxShield.SettingChanged += StatSettingChangedF;
			_levelMaxShield.SettingChanged += StatSettingChangedF;
			_baseMoveSpeed.SettingChanged += StatSettingChangedF;
			_levelMoveSpeed.SettingChanged += StatSettingChangedF;
			_baseJumpPower.SettingChanged += StatSettingChangedF;
			_levelJumpPower.SettingChanged += StatSettingChangedF;
			_sprintSpeedMultiplier.SettingChanged += StatSettingChangedF;
			_baseAcceleration.SettingChanged += StatSettingChangedF;
			_baseJumpCount.SettingChanged += StatSettingChangedI;
			_baseDamage.SettingChanged += StatSettingChangedF;
			_levelDamage.SettingChanged += StatSettingChangedF;
			_baseCritChance.SettingChanged += StatSettingChangedF;
			_levelCritChance.SettingChanged += StatSettingChangedF;
			_baseAttackSpeed.SettingChanged += StatSettingChangedF;
			_levelAttackSpeed.SettingChanged += StatSettingChangedF;
		}

		private void CameraSettingChangedNative(object sender, EventArgs args) {
			OnCameraConfigChanged?.Invoke(_cameraOffset.Value, _cameraPivotOffset.Value);
		}

		private void CameraSettingChanged(Vector3 newValue) {
			OnCameraConfigChanged?.Invoke(_cameraOffset.Value, _cameraPivotOffset.Value);
		}

		private void StatSettingChangedF(float value, bool fromNetwork) {
			OnStatConfigChanged?.Invoke();
		}

		private void StatSettingChangedI(int value, bool fromNetwork) {
			OnStatConfigChanged?.Invoke();
		}

		/// <summary>
		/// Automatically sets the getters of the <see cref="TransparencyController"/> to the backing configs for <see cref="TransparencyInCombat"/> and <see cref="TransparencyOutOfCombat"/>.
		/// </summary>
		/// <param name="controller"></param>
		public void AutoAssignTransparencyController(TransparencyController controller) {
			controller.getTransparencyInCombat = () => TransparencyInCombat;
			controller.getTransparencyOutOfCombat = () => TransparencyOutOfCombat;
		}

		/// <summary>
		/// This delegate is used in <see cref="OnStatConfigChanged"/>.
		/// </summary>
		public delegate void StatConfigChanged();

		/// <summary>
		/// This delegate is used in <see cref="OnCameraConfigChanged"/>.
		/// </summary>
		public delegate void CameraConfigChanged(Vector3 cameraOffset, float pivotHeight);

		/// <summary>
		/// Fires when any config that pertains to stats changes.
		/// </summary>
		public event StatConfigChanged OnStatConfigChanged;

		/// <summary>
		/// Fires when the camera offset or pivot height changes.
		/// </summary>
		public event CameraConfigChanged OnCameraConfigChanged;

		/// <summary>
		/// Stores everything used by a <see cref="CommonModAndCharacterConfigs"/> to load the proper default values in.
		/// </summary>
		public class Defaults {
			/// <summary>
			/// The character's starting maximum health.
			/// </summary>
			public float BaseMaxHealth { get; set; }

			/// <summary>
			/// Maximum health that the character earns every time they level up.
			/// </summary>
			public float LevelMaxHealth { get; set; }

			/// <summary>
			/// The character's starting walk speed.
			/// </summary>
			public float BaseMoveSpeed { get; set; }

			/// <summary>
			/// The additional walk speed the character earns every time they level up.
			/// </summary>
			public float LevelMoveSpeed { get; set; }

			/// <summary>
			/// The walk speed is multiplied by this value to compute the sprint speed.
			/// </summary>
			public float SprintSpeedMultiplier { get; set; }

			/// <summary>
			/// The amount of health regenerated per second by default.
			/// </summary>
			public float BaseHPRegen { get; set; }

			/// <summary>
			/// For every level the character earns, the health regeneration rate will go up by this many HP per second.
			/// </summary>
			public float LevelHPRegen { get; set; }

			/// <summary>
			/// The armor that this character starts with.
			/// </summary>
			public float BaseArmor { get; set; }

			/// <summary>
			/// The armor that is added for every level this character earns.
			/// </summary>
			public float LevelArmor { get; set; }

			/// <summary>
			/// The base damage. All attacks use this value to compute their resulting damage.
			/// </summary>
			public float BaseDamage { get; set; }

			/// <summary>
			/// Every time the character levels up, the base damage increases by this much.
			/// </summary>
			public float LevelDamage { get; set; }

			/// <summary>
			/// The default chance (from 0 to 100) that this character will land a critical hit.
			/// </summary>
			public float BaseCritChance { get; set; }

			/// <summary>
			/// Every time the character levels up, the critical hit chance increases by this much.
			/// </summary>
			public float LevelCritChance { get; set; }

			/// <summary>
			/// The default maximum shield that this character has.
			/// </summary>
			public float BaseMaxShield { get; set; }

			/// <summary>
			/// Every time this character levels up, the total shield it has gets this added to it.
			/// </summary>
			public float LevelMaxShield { get; set; }

			/// <summary>
			/// The acceleration of the character determines how quickly it reaches its move speed. Lower values make the world feel slippery.
			/// </summary>
			public float BaseAcceleration { get; set; }

			/// <summary>
			/// The amount of jumps this character has, 0 disabling jumping entirely.
			/// </summary>
			public int BaseJumpCount { get; set; }

			/// <summary>
			/// The amount of upward force applied when jumping.
			/// </summary>
			public float BaseJumpPower { get; set; }

			/// <summary>
			/// Every time the character levels up, the upward jump force is increased by this much.
			/// </summary>
			public float LevelJumpPower { get; set; }

			/// <summary>
			/// The starting attack speed that this character has.
			/// </summary>
			public float BaseAttackSpeed { get; set; }

			/// <summary>
			/// Every time this character levels up, the attack speed increases by this much.
			/// </summary>
			public float LevelAttackSpeed { get; set; }

			/// <summary>
			/// If true, do <em>not</em> downscale the character.
			/// </summary>
			public bool UseFullSizeCharacter { get; set; }

			/// <summary>
			/// Enable trace logging. Detailed logs. You get it. I'm watching youtube I don't wanna write docs anymore.
			/// </summary>
			public bool TraceLogging { get; set; }

			/// <summary>
			/// The transparency of the character model whilst in combat, as a value between 0 and 100.
			/// </summary>
			public int TransparencyInCombat { get; set; }

			/// <summary>
			/// The transparency of the character model whilst out of combat, as a value between 0 and 100.
			/// </summary>
			public int TransparencyOutOfCombat { get; set; }

			/// <summary>
			/// The vertical offset of the camera's rotation point.
			/// </summary>
			public float CameraPivotOffset { get; set; }

			/// <summary>
			/// The offset of the camera to the rotation point.
			/// </summary>
			public Vector3 CameraOffset { get; set; }
		}
	}
}
