using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.Registration {
	internal static class LanguageData {

		public const string VOID_SKIN_DEFAULT = "VOID_COMMON_CHARACTER_SKIN_DEFAULT";
		public const string VOID_SKIN_ALLY = "VOID_COMMON_CHARACTER_SKIN_ALLY";

		public static void Initialize() {
			LanguageAPI.Add(VOID_SKIN_DEFAULT, "Void Default");
			LanguageAPI.Add(VOID_SKIN_ALLY, "Newly Hatched Zoea");
		}

	}
}
