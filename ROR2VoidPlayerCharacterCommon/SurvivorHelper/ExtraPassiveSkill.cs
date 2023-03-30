#pragma warning disable Publicizer001
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.UI;
using RoR2.Skills;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {

	/// <summary>
	/// Adding this component to a <see cref="CharacterBody"/>'s <see cref="GameObject"/> will add more passive skills to the menu. These skills are incapable of doing anything.
	/// </summary>
	public class ExtraPassiveSkill : MonoBehaviour {

		/// <summary>
		/// If true, this goes to the top of the list before the standard passive skill on the <see cref="SkillLocator"/>. If false, this goes after it.
		/// </summary>
		public bool beforeNormalPassive = false;
		
		/// <summary>
		/// The extra passive skill. This is always set, edit it after adding the component.
		/// </summary>
		public SkillLocator.PassiveSkill additionalPassive = default;

	}

	internal static class ExtraPassiveHandler {

		public static void Initialize() {
			// On.RoR2.UI.CharacterSelectController.BuildSkillStripDisplayData += OnBuildingSkillStrips;

			// TO FUTURE XAN / OTHER MODDERS:
			// "Why not just use On?"
			// Because the method signature of On.RoR2.UI.CharacterSelectController.BuildSkillStripDisplayData is wrong. If you use a delegate that compiles, it fails during runtime because of a parameter mismatch.
			// Likewise, if you use a delegate that matches what it wants in runtime, it won't compile.

			IL.RoR2.UI.CharacterSelectController.BuildSkillStripDisplayData += InjectBuildSkillStripDisplayData;
		}

		private static void InjectBuildSkillStripDisplayData(ILContext il) {
			ILCursor cursor = new ILCursor(il);

			// Right at the start of the method, emit the delegate to execute beforehand. This adds extra passives to the skill window before anything else ever can.
			cursor.Emit(OpCodes.Ldarg_2);
			cursor.Emit(OpCodes.Ldarg_3);
			cursor.EmitDelegate(AddExtraPassivesPre);

			// Now find the code where the skills are added to the list. Some may want to be added after the default survivor passive is added, but before the primary/secondary/utility/special are.
			cursor.GotoNext(
				MoveType.After,
				instruction => instruction.MatchStfld(typeof(CharacterSelectController.StripDisplayData).GetField(nameof(CharacterSelectController.StripDisplayData.keywordString))),
				instruction => instruction.MatchLdloca(4),
				instruction => instruction.MatchLdstr(string.Empty),
				instruction => instruction.MatchStfld(typeof(CharacterSelectController.StripDisplayData).GetField(nameof(CharacterSelectController.StripDisplayData.actionName))),
				instruction => instruction.MatchLdloc(4),
				instruction => instruction.MatchCallvirt(out _)
			);
			cursor.Emit(OpCodes.Ldarg_2);
			cursor.Emit(OpCodes.Ldarg_3);
			cursor.EmitDelegate(AddExtraPassivesPost);
		}

		private static void AddExtraPassivesPre(in CharacterSelectController.BodyInfo bodyInfo, List<CharacterSelectController.StripDisplayData> dest) {
			if (!bodyInfo.bodyPrefab || !bodyInfo.bodyPrefabBodyComponent || !bodyInfo.skillLocator) {
				return;
			}
			AddExtraPassives(bodyInfo, dest, true);
		}

		private static void AddExtraPassivesPost(in CharacterSelectController.BodyInfo bodyInfo, List<CharacterSelectController.StripDisplayData> dest) {
			AddExtraPassives(bodyInfo, dest, false);
		}

		private static void AddExtraPassives(in CharacterSelectController.BodyInfo bodyInfo, List<CharacterSelectController.StripDisplayData> dest, bool callIsBeforePassive) {
			ExtraPassiveSkill[] extraPassives = bodyInfo.bodyPrefab.GetComponents<ExtraPassiveSkill>();
			for (int index = 0; index < extraPassives.Length; index++) {
				ExtraPassiveSkill extraPassiveSkill = extraPassives[index];
				SkillLocator.PassiveSkill skill = extraPassiveSkill.additionalPassive;
				if (!skill.enabled || !extraPassiveSkill.enabled) continue;
				if (extraPassives[index].beforeNormalPassive != callIsBeforePassive) continue;

				dest.Add(new CharacterSelectController.StripDisplayData {
					enabled = true,
					primaryColor = bodyInfo.bodyColor,
					icon = skill.icon,
					titleString = Language.GetString(skill.skillNameToken ?? string.Empty),
					descriptionString = Language.GetString(skill.skillDescriptionToken ?? string.Empty),
					keywordString = (string.IsNullOrWhiteSpace(skill.keywordToken) ? string.Empty : Language.GetString(skill.keywordToken)),
					actionName = "Passive"
				});
				Log.LogTrace($"Added extra passive skill {skill.skillNameToken}");
			}
		}
	}
}
