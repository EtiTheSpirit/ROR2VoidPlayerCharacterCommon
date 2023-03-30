#pragma warning disable Publicizer001

using R2API;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using Xan.ROR2VoidPlayerCharacterCommon.Registration;
using System.Collections.Generic;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {

	/// <summary>
	/// Contains code used by all void survivors. These utilities primarily are designed to strip monster data off of characters.
	/// </summary>
	public static class SurvivorHollower {

		private static readonly List<Material> _allVoidMaterials = new List<Material>();
		private static readonly Dictionary<string, string> _alreadyRegisteredNames = new Dictionary<string, string>();


		/// <summary>
		/// Creates a new container for a <see cref="CharacterBody"/> and sets up its default, blank skills.
		/// </summary>
		/// <param name="bodyReplacementName">The name of the body prefab.</param>
		/// <param name="bodyDir">The location of the body prefab.</param>
		/// <param name="allyBodyDir">The location of the ally body prefab, which is used to sample the skin.</param>
		/// <returns></returns>
		public static GameObject CreateBodyWithSkins(string bodyReplacementName, string bodyDir, string allyBodyDir) {
			Log.LogTrace($"Creating new CharacterBody of {bodyDir} as {bodyReplacementName}");
			GameObject newBody = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(bodyDir).WaitForCompletion(), bodyReplacementName);
			GameObject existingAllyBody = Addressables.LoadAssetAsync<GameObject>(allyBodyDir).WaitForCompletion();

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
			SkillFamily primaryFamily = CreateSingleVariantFamily("primary");
			SkillFamily secondaryFamily = CreateSingleVariantFamily("secondary");
			SkillFamily utilityFamily = CreateSingleVariantFamily("utility");
			SkillFamily specialFamily = CreateSingleVariantFamily("special");

			Log.LogTrace("Assigning empty skills...");
			skillLocator.primary._skillFamily = primaryFamily;
			skillLocator.secondary._skillFamily = secondaryFamily;
			skillLocator.utility._skillFamily = utilityFamily;
			skillLocator.special._skillFamily = specialFamily;

			Log.LogTrace("Body instantiated. Appending skins...");
			RegisterSkins(newBody, existingAllyBody);

			Log.LogTrace("Done creating skins. ");
			return newBody;
		}

		/// <summary>
		/// Intended to be called after the Body has all of its stuff attached, this prevents an error in the console that would
		/// result in the rejection of these skill families being added to game data.
		/// </summary>
		/// <param name="skillLocator"></param>
		public static void FinalizeBody(SkillLocator skillLocator) {
			Log.LogTrace("Finalizing body by using ContentAddition to register all skills...");
			skillLocator.allSkills = skillLocator.gameObject.GetComponents<GenericSkill>();
			/*
			ContentAddition.AddSkillFamily(skillLocator.primary._skillFamily);
			ContentAddition.AddSkillFamily(skillLocator.secondary._skillFamily);
			ContentAddition.AddSkillFamily(skillLocator.utility._skillFamily);
			ContentAddition.AddSkillFamily(skillLocator.special._skillFamily);
			*/
			foreach (GenericSkill def in skillLocator.allSkills) {
				// if (def == skillLocator.primary || def == skillLocator.secondary || def == skillLocator.utility || def == skillLocator.special) continue;
				SkillFamily family = def._skillFamily;
				if (string.IsNullOrEmpty(UnityEngine.Object.GetName(family).Trim())) {
					Log.LogError("ALERT: A skill family has no name. For obvious reasons, it's not possible to tell you which one it is, because it's nameless. You should probably add a name to all of your skill families, otherwise it won't save properly.");
					// "but it errors when i try to set it!!!" no
					// do the thing it tells you to do, cast to ScriptableObject d u m m y
				}
				ContentAddition.AddSkillFamily(def._skillFamily);
			}
		}


		/// <summary>
		/// Creates a <see cref="SkillFamily"/> with one variant slot. It has no other data defined.
		/// </summary>
		/// <returns></returns>
		private static SkillFamily CreateSingleVariantFamily(string familyName) {
			// No ContentAddition here!
			Log.LogTrace("Creating single-variant SkillFamily.");
			SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
			skillFamily.variants = new SkillFamily.Variant[1];
			((ScriptableObject)skillFamily).name = familyName; // Needed for data persistence. The cast is needed as its Name property is hidden to error on access to encourage using the lookup. It says to use this technique to bypass the limit.
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
			Log.LogTrace($"Adding a skill to this character's {slotType} skill slot. Verifying data integrity and overriding persistent name to be its localization key...");
			((ScriptableObject)definition).name = definition.skillNameToken; // This is needed for data persistence. The cast is needed as its Name property is hidden to error on access to encourage using the lookup. It says to use this technique to bypass the limit.
			definition.skillName = definition.skillNameToken;

			if (_alreadyRegisteredNames.TryGetValue(definition.skillName, out string registrarLocalizationKey)) throw new InvalidOperationException($"VOID COMMON API // MALFORMED DATA: Body [{bodyContainer}] attempted to register skill \"{definition.skillNameToken}\" with a persistent name of \"{definition.skillName}\", but this unique name was already reserved previously by skill \"{registrarLocalizationKey}\"! This MUST be fixed before the loading cycle can continue.");
			
			Log.LogTrace("Registering skill to ContentAddition...");
			ContentAddition.AddSkillDef(definition);
			_alreadyRegisteredNames[definition.skillName] = definition.skillNameToken;

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
				case SlotType.Extra:
					target = bodyContainer.AddComponent<GenericSkill>();
					target._skillFamily ??= ScriptableObject.CreateInstance<SkillFamily>();
					((ScriptableObject)target._skillFamily).name = definition.skillNameToken + "$EXTRA_FAMILY";
					target.skillFamily.variants ??= Array.Empty<SkillFamily.Variant>();
					break;
				case SlotType.ExtraHidden:
					target = bodyContainer.AddComponent<GenericSkill>();
					target._skillFamily ??= ScriptableObject.CreateInstance<SkillFamily>();
					((ScriptableObject)target._skillFamily).name = definition.skillNameToken + "$EXTRA_FAMILY_HIDDEN";
					target.skillFamily.variants ??= Array.Empty<SkillFamily.Variant>();
					target.hideInCharacterSelect = true;
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
			newVariant.viewableNode = (slotType == SlotType.ExtraHidden) ? null : (new ViewablesCatalog.Node(definition.skillName + "_VIEW", false, null));
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
		/// <param name="survivorObject">The survivor's body prefab.</param>
		/// <param name="dataSrcBodyPrefab">The body to take data from to copy skins.</param>
		/// <param name="skinNameToken"></param>
		/// <param name="isFriendly">If true, the alternate icon gets used on the skin.</param>
		private static LoadoutAPI.SkinDefInfo CreateDefaultAndZoeaSkin(GameObject survivorObject, GameObject dataSrcBodyPrefab, string skinNameToken, bool isFriendly) {
			Renderer[] originalRenderers = survivorObject.GetComponentsInChildren<Renderer>();
			Renderer[] dataSrcRenderers = dataSrcBodyPrefab.GetComponentsInChildren<Renderer>();
			GameObject originalModelRoot = survivorObject.GetComponent<ModelLocator>().modelTransform.gameObject;

			Log.LogTrace("Cloning the default materials of the character...");
			Sprite icon;
			if (isFriendly) {
				icon = SkinIconCreator.CreateSkinIcon(
					new Color32(183, 172, 175, 255),
					new Color32(78, 117, 145, 255),
					new Color32(152, 151, 227, 255),
					new Color32(54, 169, 226, 255)
				);
			} else {
				icon = SkinIconCreator.CreateSkinIcon(
					new Color32(24, 1, 33, 255),
					new Color32(52, 84, 108, 255),
					new Color32(239, 151, 227, 255),
					new Color32(11, 34, 127, 255)
				);
			}
			LoadoutAPI.SkinDefInfo newSkin = new LoadoutAPI.SkinDefInfo {
				Icon = icon,
				NameToken = skinNameToken,
				RootObject = originalModelRoot,
				BaseSkins = Array.Empty<SkinDef>(),
				GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>(),
				ProjectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>(),
				MinionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>(),
				RendererInfos = new CharacterModel.RendererInfo[originalRenderers.Length]
			};

			for (int i = 0; i < newSkin.RendererInfos.Length; i++) {
				Material replacement = new Material(dataSrcRenderers[i].material); // yes, dataSrcRenderers here (copy from data source)
				newSkin.RendererInfos[i] = new CharacterModel.RendererInfo {
					defaultMaterial = replacement,
					defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
					ignoreOverlays = false,
					renderer = originalRenderers[i] // and originalRenderers here (set the affected reference to that of the model on my survivor)
				};
				_allVoidMaterials.Add(replacement);
			}

			return newSkin;
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
			}
			ctrl.skins = Array.Empty<SkinDef>();

			Log.LogTrace("Registering skins with LoadoutAPI...");
			LoadoutAPI.AddSkinToCharacter(bodyPrefab, CreateDefaultAndZoeaSkin(bodyPrefab, bodyPrefab, LanguageData.VOID_SKIN_DEFAULT, false));
			LoadoutAPI.AddSkinToCharacter(bodyPrefab, CreateDefaultAndZoeaSkin(bodyPrefab, allyOriginalPrefab, LanguageData.VOID_SKIN_ALLY, true));
		}

		/// <summary>
		/// Should be called on mannequin rebuild to disable visible transparency in the character selection screen.
		/// </summary>
		private static void DisallowMaterialTransparency() {
			foreach (Material mtl in _allVoidMaterials) {
				SetTransparencyEnabled(mtl, false);
			}
		}

		/// <summary>
		/// Should be called on CharacterBody awake to allow visible transparency in the game.
		/// </summary>
		private static void AllowMaterialTransparency() {
			foreach (Material mtl in _allVoidMaterials) {
				SetTransparencyEnabled(mtl, true);
			}
		}

		private static void SetTransparencyEnabled(Material mtl, bool enabled) {
			if (enabled) {
				mtl.EnableKeyword("DITHER");
				mtl.SetFloat("_DitherOn", 1f);
			} else {
				mtl.DisableKeyword("DITHER");
				mtl.SetFloat("_DitherOn", 0f);
			}
		}

		internal static void Initialize() {
			On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.RebuildMannequinInstance += OnRebuildingMannequins;
			On.RoR2.CharacterBody.Awake += OnCharacterBodyAwakened;
		}

		private static void OnCharacterBodyAwakened(On.RoR2.CharacterBody.orig_Awake originalMethod, CharacterBody @this) {
			originalMethod(@this);
			AllowMaterialTransparency();
		}

		private static void OnRebuildingMannequins(On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.orig_RebuildMannequinInstance originalMethod, RoR2.SurvivorMannequins.SurvivorMannequinSlotController @this) {
			originalMethod(@this);
			DisallowMaterialTransparency();
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
			Special,

			/// <summary>
			/// An arbitrary skill bound to an arbitrary "Misc" slot.
			/// </summary>
			Extra,

			/// <summary>
			/// An arbitrary skill that doesn't show up in the "Skills" menu (but <em>does</em> show in the "Loadout" menu).
			/// </summary>
			ExtraHidden,

		}

	}
}
