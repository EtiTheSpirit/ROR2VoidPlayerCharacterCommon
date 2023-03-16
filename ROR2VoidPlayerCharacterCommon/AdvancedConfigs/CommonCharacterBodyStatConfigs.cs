using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {

	/// <summary>
	/// Some configuration values, such as base stats, are common across all void characters. Construct an instance of this class to automatically register and create all of these options.
	/// </summary>
	public class CommonCharacterBodyStatConfigs {


		#region Base Stats
		/// <summary>
		/// The character's starting maximum health.
		/// </summary>
		public float BaseMaxHealth => _baseMaxHealth.Value;

		/// <summary>
		/// Maximum health that the character earns every time they level up.
		/// </summary>
		public float LevelMaxHealth => _levelMaxHealth.Value;

		/// <summary>
		/// The character's starting walk speed.
		/// </summary>
		public float BaseMoveSpeed => _baseMoveSpeed.Value;

		/// <summary>
		/// The additional walk speed the character earns every time they level up.
		/// </summary>
		public float LevelMoveSpeed => _levelMoveSpeed.Value;

		/// <summary>
		/// The walk speed is multiplied by this value to compute the sprint speed.
		/// </summary>
		public float SprintSpeedMultiplier => _sprintSpeedMultiplier.Value;

		/// <summary>
		/// The amount of health regenerated per second by default.
		/// </summary>
		public float BaseHPRegen => _baseHPRegen.Value;

		/// <summary>
		/// For every level the character earns, the health regeneration rate will go up by this many HP per second.
		/// </summary>
		public float LevelHPRegen => _levelHPRegen.Value;

		/// <summary>
		/// The armor that this character starts with.
		/// </summary>
		public float BaseArmor => _baseArmor.Value;

		/// <summary>
		/// The armor that is added for every level this character earns.
		/// </summary>
		public float LevelArmor => _levelArmor.Value;

		/// <summary>
		/// The base damage. All attacks use this value to compute their resulting damage.
		/// </summary>
		public float BaseDamage => _baseDamage.Value;

		/// <summary>
		/// Every time the character levels up, the base damage increases by this much.
		/// </summary>
		public float LevelDamage => _levelDamage.Value;

		/// <summary>
		/// The default chance (from 0 to 100) that this character will land a critical hit.
		/// </summary>
		public float BaseCritChance => _baseCritChance.Value;

		/// <summary>
		/// Every time the character levels up, the critical hit chance increases by this much.
		/// </summary>
		public float LevelCritChance => _levelCritChance.Value;

		/// <summary>
		/// The default maximum shield that this character has.
		/// </summary>
		public float BaseMaxShield => _baseMaxShield.Value;

		/// <summary>
		/// Every time this character levels up, the total shield it has gets this added to it.
		/// </summary>
		public float LevelMaxShield => _levelMaxShield.Value;

		/// <summary>
		/// The acceleration of the character determines how quickly it reaches its move speed. Lower values make the world feel slippery.
		/// </summary>
		public float BaseAcceleration => _baseAcceleration.Value;

		/// <summary>
		/// The amount of jumps this character has, 0 disabling jumping entirely.
		/// </summary>
		public int BaseJumpCount => _baseJumpCount.Value;

		/// <summary>
		/// The amount of upward force applied when jumping.
		/// </summary>
		public float BaseJumpPower => _baseJumpPower.Value;

		/// <summary>
		/// Every time the character levels up, the upward jump force is increased by this much.
		/// </summary>
		public float LevelJumpPower => _levelJumpPower.Value;

		/// <summary>
		/// The starting attack speed that this character has.
		/// </summary>
		public float BaseAttackSpeed => _baseAttackSpeed.Value;

		/// <summary>
		/// Every time this character levels up, the attack speed increases by this much.
		/// </summary>
		public float LevelAttackSpeed => _levelAttackSpeed.Value;

		/// <summary>
		/// If true, do <em>not</em> downscale the character.
		/// </summary>
		public bool UseFullSizeCharacter => _useFullSizeCharacter.Value;

		/// <summary>
		/// Enable trace logging. Detailed logs. You get it. I'm watching youtube I don't wanna write docs anymore.
		/// </summary>
		public bool TraceLogging => _traceLogging.Value;

		/// <summary>
		/// The transparency of the character model whilst in combat, as a value between 0 and 100.
		/// </summary>
		public int TransparencyInCombat => _transparencyInCombat.Value;

		/// <summary>
		/// The transparency of the character model whilst out of combat, as a value between 0 and 100.
		/// </summary>
		public int TransparencyOutOfCOmbat => _transparencyOutOfCombat.Value;

		#endregion


		#region Backing
		#region Base Stats
		private ConfigEntry<float> _baseMaxHealth;
		private ConfigEntry<float> _levelMaxHealth;

		private ConfigEntry<float> _baseMoveSpeed;
		private ConfigEntry<float> _levelMoveSpeed;

		private ConfigEntry<float> _sprintSpeedMultiplier;

		private ConfigEntry<float> _baseHPRegen;
		private ConfigEntry<float> _levelHPRegen;

		private ConfigEntry<float> _baseArmor;
		private ConfigEntry<float> _levelArmor;

		private ConfigEntry<float> _baseDamage;
		private ConfigEntry<float> _levelDamage;

		private ConfigEntry<float> _baseCritChance;
		private ConfigEntry<float> _levelCritChance;

		private ConfigEntry<float> _baseMaxShield;
		private ConfigEntry<float> _levelMaxShield;

		private ConfigEntry<float> _baseAcceleration;

		private ConfigEntry<int> _baseJumpCount;

		private ConfigEntry<float> _baseJumpPower;
		private ConfigEntry<float> _levelJumpPower;

		private ConfigEntry<float> _baseAttackSpeed;
		private ConfigEntry<float> _levelAttackSpeed;

		#endregion

		#region Misc.
		private ConfigEntry<bool> _useFullSizeCharacter;
		private ConfigEntry<bool> _traceLogging;
		private ConfigEntry<int> _transparencyInCombat;
		private ConfigEntry<int> _transparencyOutOfCombat;
		#endregion
		#endregion

		private const string FMT_DEFAULT = "The base {0} that the character has on a new run.";
		private const string FMT_LEVELED = "For each level the player earns, the base {0} increases by this amount.";
		private const string FMT_TRANSPARENCY = "The transparency of the character when you are {0}.\n\nThis can be used to make it easier to see enemies by making your body transparent to prevent it from getting in the way.\n\nA value of 0 means fully opaque, and a value of 100 means as invisible as possible.";

		private static void MakeBaseAndLeveled(AdvancedConfigBuilder aCfg, string statName, float defaultBase, float defaultLeveled, ref ConfigEntry<float> @base, ref ConfigEntry<float> leveled) {
			@base = aCfg.Bind($"Base {statName}", defaultBase, string.Format(FMT_DEFAULT, statName), restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
			leveled = aCfg.Bind($"Leveled {statName}", defaultLeveled, string.Format(FMT_LEVELED, statName), restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
		}

		/// <summary>
		/// Using the provided <see cref="AdvancedConfigBuilder"/>, this will register all underlying <see cref="CharacterBody"/> stats as config entries.
		/// </summary>
		/// <param name="aCfg"></param>
		public CommonCharacterBodyStatConfigs(AdvancedConfigBuilder aCfg) {

			// The odd one out:
			aCfg.SetCategory("Mod Meta, Graphics, Gameplay");
			_traceLogging = aCfg.Bind("Trace Logging", false, "Trace Logging is a practice where individual steps through the code are logged. This can dramatically increase the size and clutter within your log file, and should only really be used when debugging.");
			_transparencyInCombat = aCfg.Bind("Transparency In Combat", 0, string.Format(FMT_TRANSPARENCY, "in combat"), restartRequired: AdvancedConfigBuilder.RestartType.NoRestartRequired);
			_transparencyOutOfCombat = aCfg.Bind("Transparency Out Of Combat", 0, string.Format(FMT_TRANSPARENCY, "not in combat"), restartRequired: AdvancedConfigBuilder.RestartType.NoRestartRequired);
			_useFullSizeCharacter = aCfg.Bind("Use Full Size Character", false, "By default, the player character has its size reduced to 50% its normal scale. Enabling this option will use the full size character model.\n\n<style=cDeath>Warning:</style> Using the full size character is known to cause some problems, especially in that it makes certain areas of the map impossible to access.");
			//_traceLogging = cfg.Bind("6. Other Options", "Trace Logging", false, "If true, trace logging is enabled. Your console will practically be spammed as the mod gives status updates on every little thing it's doing, but it will help to diagnose weird issues. Consider using this when bug hunting!");

			aCfg.SetCategory("Health, Armor, Shield");
			MakeBaseAndLeveled(aCfg, "Maximum Health", 220f, 40f, ref _baseMaxHealth, ref _levelMaxHealth);
			MakeBaseAndLeveled(aCfg, "Health Regeneration Rate", 1f, 0.2f, ref _baseHPRegen, ref _levelHPRegen);
			MakeBaseAndLeveled(aCfg, "Armor", 12f, 0f, ref _baseArmor, ref _levelArmor);
			MakeBaseAndLeveled(aCfg, "Shield", 0f, 0f, ref _baseMaxShield, ref _levelMaxShield);

			aCfg.SetCategory("Mobility and Agility");
			MakeBaseAndLeveled(aCfg, "Walk Speed", 7f, 0f, ref _baseMoveSpeed, ref _levelMoveSpeed);
			MakeBaseAndLeveled(aCfg, "Jump Strength", 1f, 0f, ref _baseJumpPower, ref _levelJumpPower);
			_sprintSpeedMultiplier = aCfg.Bind("Sprint Speed Multiplier", 1.45f, "Your sprint speed is equal to your Base Walk Speed times this value.", restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
			_baseAcceleration = aCfg.Bind("Acceleration", 80f, "Acceleration determines how quickly your character gets up to your move speed. Low values make it like walking on ice.", restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);
			_baseJumpCount = aCfg.Bind("Jump Count", 1, "The amount of jumps your character has. 1 is a single jump, 2 is a double jump, etc.", restartRequired: AdvancedConfigBuilder.RestartType.NextRespawn);

			aCfg.SetCategory("Underlying Combat Stats");
			MakeBaseAndLeveled(aCfg, "Base Damage", 20f, 2.4f, ref _baseDamage, ref _levelDamage);
			MakeBaseAndLeveled(aCfg, "Crit Chance", 1f, 0f, ref _baseCritChance, ref _levelCritChance);
			MakeBaseAndLeveled(aCfg, "Attack Speed", 1f, 0f, ref _baseAttackSpeed, ref _levelAttackSpeed);
		}

	}
}
