using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static R2API.DamageAPI;

namespace Xan.ROR2VoidPlayerCharacterCommon.DamageBehavior {

	/// <summary>
	/// This class contains common damage types used by various Void-related attacks.
	/// </summary>
	public static class VoidDamageTypes {

		/// <summary>
		/// This damage type is applied when the damage should have the <em>>visual appearance</em> of <see cref="DamageType.VoidDeath"/>, but should <em>not</em> instantly kill the target. This means
		/// the target can take damage normally, but upon death, will be deleted like with a Void death. If the target is immune to Void death, this will behave like standard damage.
		/// </summary>
		public static ModdedDamageType VoidDeathOnDeath { get; private set; }

		/// <summary>
		/// This damage type can be applied to tell the Exaggerated Void Death mod's effects to not apply. This should strictly be used iff the incoming damage type has <see cref="DamageType.VoidDeath"/>.
		/// If the damage type has <see cref="VoidDeathOnDeath"/> and you use this, you are literally applying the thing and then canceling it out. Just remove it 16384head.
		/// </summary>
		public static ModdedDamageType BlacklistExaggeratedVoidDeath { get; private set; }

		internal static void Initialize() {
			Log.LogInfo("Initializing Void Damage Types...");
			Log.LogTrace($"Visual Void Death ({nameof(VoidDamageTypes)}::{nameof(VoidDeathOnDeath)})...");
			VoidDeathOnDeath = ReserveDamageType();
			Log.LogTrace($"Exaggerated Void Death Blacklist ({nameof(VoidDamageTypes)}::{nameof(BlacklistExaggeratedVoidDeath)})...");
			BlacklistExaggeratedVoidDeath = ReserveDamageType();
		}

	}
}
