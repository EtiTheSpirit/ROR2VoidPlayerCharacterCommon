using RoR2;
using ROR2HPBarAPI.API;
using ROR2HPBarAPI.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {
	internal class VoidStyleHPBarTheme : AbstractColorProvider {

		public static VoidStyleHPBarTheme Instance => new VoidStyleHPBarTheme();

		public override void UpdateBarColors(CharacterBody sourceBody, DesiredBarColorData barColorData) {
			barColorData.OverrideShieldColor = DefaultHealthAndShieldData.VoidShield;
			barColorData.OverrideHealthColor = DefaultHealthAndShieldData.VoidHealth;
			barColorData.OverrideBarrierColor = new Color32(255, 0, 127, 255);
			barColorData.OverrideCullBar = new Color32(166, 124, 133, 255);
			barColorData.OverrideHealingColor = new Color32(94, 88, 214, 255);
			barColorData.OverridePainColor = new Color32(122, 142, 158, 255);
			barColorData.OverrideLowHealthBacking = new Color32(30, 0, 43, 255);
			barColorData.OverrideLowHealthFlashColor1 = new Color32(163, 60, 207, 255);
			barColorData.OverrideLowHealthFlashColor2 = new Color32(205, 154, 227, 255);
			barColorData.OverrideLowHealthOverlay = new Color32(255, 255, 255, 255);
		}

		public override void UpdateShieldOverrides(CharacterBody sourceBody, DesiredShieldRenderData shieldRenderData) {
			shieldRenderData.OverrideShieldOverlayColor = DefaultHealthAndShieldData.VoidShieldOverlayColor;
			shieldRenderData.OverrideShieldBoost = DefaultHealthAndShieldData.VoidShieldOverlayBoost;
			shieldRenderData.OverrideBarrierColor = new Color(0.5f, 0, 4);
			shieldRenderData.ShieldIsDynamic = false;
			shieldRenderData.BarrierIsDynamic = false;
		}

	}
}
