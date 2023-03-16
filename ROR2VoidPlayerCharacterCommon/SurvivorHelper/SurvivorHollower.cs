#pragma warning disable IDE0066 // The version of C# we are using does not support switch expressions.
#pragma warning disable Publicizer001

using R2API;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using Xan.ROR2VoidPlayerCharacterCommon.Registration;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {

	/// <summary>
	/// Contains code used by all void survivors. These utilities primarily are designed to strip monster data off of characters.
	/// </summary>
	public static class SurvivorHollower {


		/// <summary>
		/// Designed by LuaFubuki, modified by Xan.<br/>
		/// Creates a new container for a <see cref="CharacterBody"/> and sets up its default, blank skills.
		/// </summary>
		/// <param name="bodyReplacementName">The name of the body prefab.</param>
		/// <param name="bodyDir">The location of the body prefab.</param>
		/// <param name="allyBodyDir">The location of the ally body prefab, which is used to sample the skin.</param>
		/// <returns></returns>
		public static GameObject CreateBodyWithSkins(string bodyReplacementName, string bodyDir, string allyBodyDir) {
			Log.LogTrace($"Creating new CharacterBody of {bodyDir} as {bodyReplacementName}");
			GameObject newBody = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(bodyDir).WaitForCompletion(), bodyReplacementName);
			GameObject newAllyBody = Addressables.LoadAssetAsync<GameObject>(allyBodyDir).WaitForCompletion();

			Log.LogTrace("Destroying all pre-existing skill instances of the duplicate body...");
			foreach (GenericSkill preExistingSkill in newBody.GetComponentsInChildren<GenericSkill>()) {
				UnityEngine.Object.DestroyImmediate(preExistingSkill);
			}

			Log.LogTrace("Clearing DeathRewards (if present)...");
			// Prevent it from being classified as a monster.
			DeathRewards deathRewards = newBody.GetComponent<DeathRewards>();
			if (deathRewards != null) UnityEngine.Object.Destroy(deathRewards);

			Log.LogTrace("Clearing SkillLocator for new skills...");
			SkillLocator skillLocator = newBody.GetComponent<SkillLocator>();
			skillLocator.allSkills = Array.Empty<GenericSkill>();
			skillLocator.primary = newBody.AddComponent<GenericSkill>();
			skillLocator.secondary = newBody.AddComponent<GenericSkill>();
			skillLocator.utility = newBody.AddComponent<GenericSkill>();
			skillLocator.special = newBody.AddComponent<GenericSkill>();

			Log.LogTrace("Adding single-variant placeholder skills...");
			SkillFamily primaryFamily = CreateSingleVariantFamily();
			SkillFamily secondaryFamily = CreateSingleVariantFamily();
			SkillFamily utilityFamily = CreateSingleVariantFamily();
			SkillFamily specialFamily = CreateSingleVariantFamily();

			Log.LogTrace("Assigning empty skills...");
			skillLocator.primary._skillFamily = primaryFamily;
			skillLocator.secondary._skillFamily = secondaryFamily;
			skillLocator.utility._skillFamily = utilityFamily;
			skillLocator.special._skillFamily = specialFamily;

			Log.LogTrace("Body instantiated. Appending skins...");
			RegisterSkins(newBody, newAllyBody);

			Log.LogTrace("Done creating skins. ");
			return newBody;
		}

		/// <summary>
		/// Added by Xan.
		/// Intended to be called after the Body has all of its stuff attached, this prevents an error in the console that would
		/// result in the rejection of these skill families being added to game data.
		/// </summary>
		/// <param name="skillLocator"></param>
		public static void FinalizeBody(SkillLocator skillLocator) {
			Log.LogTrace("Finalizing body by using ContentAddition to register all skills...");
			ContentAddition.AddSkillFamily(skillLocator.primary._skillFamily);
			ContentAddition.AddSkillFamily(skillLocator.secondary._skillFamily);
			ContentAddition.AddSkillFamily(skillLocator.utility._skillFamily);
			ContentAddition.AddSkillFamily(skillLocator.special._skillFamily);
		}


		/// <summary>
		/// Creates a <see cref="SkillFamily"/> with one variant slot. It has no other data defined.
		/// </summary>
		/// <returns></returns>
		private static SkillFamily CreateSingleVariantFamily() {
			// No ContentAddition here!
			Log.LogTrace("Creating single-variant SkillFamily.");
			SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
			skillFamily.variants = new SkillFamily.Variant[1];
			return skillFamily;
		}

		/// <summary>
		/// Adds a skill variant to the given slot of a <see cref="CharacterBody"/>-containing <see cref="GameObject"/>.
		/// </summary>
		/// <param name="bodyContainer">The <see cref="GameObject"/> that has a <see cref="CharacterBody"/> component on it.</param>
		/// <param name="definition">The actual skill to add.</param>
		/// <param name="slotType">The slot to put this skill into.</param>
		/// <param name="variantIndex">The index of this variant. If this index is larger than the number of variants the <see cref="SkillFamily"/> can contain, its array is resized.</param>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="slotType"/> is not valid.</exception>
		public static void AddSkill(GameObject bodyContainer, SkillDef definition, SlotType slotType, int variantIndex = 0) {
			Log.LogTrace($"Adding a skill to this character's {slotType} skill slot. Registering skill to ContentAddition...");
			ContentAddition.AddSkillDef(definition);

			SkillLocator skillLocator = bodyContainer.GetComponent<SkillLocator>();
			GenericSkill target;
			switch (slotType) {
				case SlotType.Primary:
					target = skillLocator.primary;
					break;
				case SlotType.Secondary:
					target = skillLocator.secondary;
					break;
				case SlotType.Utility:
					target = skillLocator.utility;
					break;
				case SlotType.Special:
					target = skillLocator.special;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(slotType), "Invalid slot type!");
			}

			Log.LogTrace("Locating Skill Family...");
			SkillFamily family = target.skillFamily;
			SkillFamily.Variant[] variants = family.variants;
			if (variants.Length <= variantIndex) {
				Log.LogTrace("Expanding Skill Family Variants array...");
				Array.Resize(ref variants, variantIndex + 1);
			}
			SkillFamily.Variant newVariant = default;
			newVariant.skillDef = definition;
			newVariant.viewableNode = new ViewablesCatalog.Node(definition.skillName + "_VIEW", false, null);
			variants[variantIndex] = newVariant;
			family.variants = variants;
			Log.LogTrace($"Done. Appended new skill in slot \"{slotType}\": {definition.skillNameToken}");
		}

		/// <summary>
		/// Adds a new skill to the character. This skill is not actually shown, it is only registered.
		/// </summary>
		/// <param name="definition"></param>
		public static void AddNewHiddenSkill(SkillDef definition) {
			Log.LogTrace($"Adding a hidden skill to this character. Registering skill to ContentAddition...");
			ContentAddition.AddSkillDef(definition);

			Log.LogTrace("Setting Skill Family...");
			SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
			SkillFamily.Variant[] variants = family.variants;

			Log.LogTrace("Resizing Skill Family Variants array...");
			Array.Resize(ref variants, 1);

			SkillFamily.Variant newVariant = default;
			newVariant.skillDef = definition;
			newVariant.viewableNode = null;
			variants[0] = newVariant;
			family.variants = variants;
			ContentAddition.AddSkillFamily(family);
			Log.LogTrace($"Done. Appended new hidden skill in no slot: {definition.skillNameToken}");
		}

		/// <summary>
		/// Translates the stock Void skin and the Newly Hatched Zoea skin into variants for the survivor.
		/// </summary>
		/// <param name="bodyPrefab"></param>
		/// <param name="skinNameToken"></param>
		private static LoadoutAPI.SkinDefInfo CreateDefaultAndZoeaSkin(GameObject bodyPrefab, string skinNameToken) {
			Renderer[] renderers = bodyPrefab.GetComponentsInChildren<Renderer>();
			ModelLocator component = bodyPrefab.GetComponent<ModelLocator>();
			GameObject effectiveRoot = component.modelTransform.gameObject;

			Log.LogTrace("Cloning the default materials of the character...");
			LoadoutAPI.SkinDefInfo defaultSkin = new LoadoutAPI.SkinDefInfo {
				Icon = SkinIconCreator.CreateSkinIcon(
					new Color32(24, 1, 33, 255),
					new Color32(52, 84, 108, 255),
					new Color32(239, 151, 227, 255),
					new Color32(11, 34, 127, 255)
				),
				NameToken = skinNameToken,
				RootObject = effectiveRoot,
				BaseSkins = Array.Empty<SkinDef>(),
				GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>(),
				ProjectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>(),
				MinionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>(),
				RendererInfos = new CharacterModel.RendererInfo[renderers.Length]
			};

			Material[] mtls = new Material[renderers.Length];
			for (int i = 0; i < mtls.Length; i++) {
				defaultSkin.RendererInfos[i] = new CharacterModel.RendererInfo {
					defaultMaterial = new Material(mtls[i]),
					defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
					ignoreOverlays = false,
					renderer = renderers[i]
				};
			}

			return defaultSkin;
		}

		/// <summary>
		/// Register skins for the default and friendly variants of this void entity.
		/// </summary>
		/// <param name="bodyPrefab">The cloned body prefab being used for the survivor.</param>
		/// <param name="allyOriginalPrefab">A reference to the original, uncloned, vanilla ally model (for reference).</param>
		private static void RegisterSkins(GameObject bodyPrefab, GameObject allyOriginalPrefab) {
			ModelLocator component = bodyPrefab.GetComponent<ModelLocator>();
			GameObject effectiveRoot = component.modelTransform.gameObject;

			Log.LogTrace("Adding skin controller, if necessary...");
			ModelSkinController ctrl = effectiveRoot.GetComponent<ModelSkinController>();
			bool justCreatedController = ctrl == null;
			if (justCreatedController) {
				ctrl = effectiveRoot.AddComponent<ModelSkinController>();
				ctrl.characterModel = bodyPrefab.GetComponent<CharacterModel>();
				ctrl.skins = Array.Empty<SkinDef>();
			}

			Log.LogTrace("Registering skins with LoadoutAPI...");
			LoadoutAPI.AddSkinToCharacter(bodyPrefab, CreateDefaultAndZoeaSkin(bodyPrefab, LanguageData.VOID_SKIN_DEFAULT));
			LoadoutAPI.AddSkinToCharacter(bodyPrefab, CreateDefaultAndZoeaSkin(allyOriginalPrefab, LanguageData.VOID_SKIN_ALLY));
		}

		/// <summary>
		/// Represents a type of slot used by a character.
		/// </summary>
		public enum SlotType {
			
			/// <summary>
			/// The primary slot.
			/// </summary>
			Primary,

			/// <summary>
			/// The secondary slot.
			/// </summary>
			Secondary,

			/// <summary>
			/// The utility slot.
			/// </summary>
			Utility,

			/// <summary>
			/// The special slot.
			/// </summary>
			Special

		}

	}
}
