using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.Registration {
	internal static class Helpers {

		internal static string ModToString(BaseUnityPlugin plugin) {
			BepInPlugin bep = plugin.Info.Metadata;
			return $"{bep.Name} (ID: {bep.GUID})";
		}

		internal static string BodyToString(BodyIndex index) {
			return $"CharacterBody #{(int)index} ({BodyCatalog.GetBodyName(index)})";
		}

		internal static string BodyToString(CharacterBody body) {
			return $"CharacterBody #{(int)body.bodyIndex} ({body.name})";
		}
	}
}
