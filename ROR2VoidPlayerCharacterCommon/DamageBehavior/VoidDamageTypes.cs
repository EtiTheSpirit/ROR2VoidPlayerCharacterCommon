﻿using R2API;
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
		/// This damage type should only be applied when the damage should (and would not normally) have the <em>visual appearance</em> of <see cref="DamageType.VoidDeath"/>.
		/// <para/>
		/// <strong>Do not</strong> add this damage type if it either:<br/>
		/// 1: Already includes <see cref="DamageType.VoidDeath"/>, or<br/>
		/// 2: Uses <see cref="BlacklistExaggeratedVoidDeath"/>
		/// </summary>
		public static ModdedDamageType DisplayVoidDeathOnDeath { get; private set; }

		/// <summary>
		/// This damage type can be applied to tell the Exaggerated Void Death mod's effects to not apply. This should strictly be used iff the incoming damage type has <see cref="DamageType.VoidDeath"/>.
		/// If the damage type has <see cref="DisplayVoidDeathOnDeath"/> and you use this, you are literally applying the thing and then canceling it out. Just remove it 16384head.
		/// </summary>
		public static ModdedDamageType BlacklistExaggeratedVoidDeath { get; private set; }

		/// <summary>
		/// This damage type should only be applied to a <see cref="DamageInfo"/> whose type already includes <see cref="DamageType.VoidDeath"/>. This makes the death "conditional" in that it will use
		/// <see cref="XanVoidAPI.CanCharacterInstakillMonsters(BodyIndex)"/> (and the related methods) to figure out whether or not it should actually apply as <see cref="DamageType.VoidDeath"/>.
		/// </summary>
		public static ModdedDamageType ConditionalVoidDeath { get; private set; }

		internal static void Initialize() {
			Log.LogInfo("Initializing Void Damage Types...");
			Log.LogTrace($"Visual Void Death ({nameof(VoidDamageTypes)}::{nameof(DisplayVoidDeathOnDeath)})...");
			DisplayVoidDeathOnDeath = ReserveDamageType();
			Log.LogTrace($"Exaggerated Void Death Blacklist ({nameof(VoidDamageTypes)}::{nameof(BlacklistExaggeratedVoidDeath)})...");
			BlacklistExaggeratedVoidDeath = ReserveDamageType();
			Log.LogTrace($"Conditional Void Death ({nameof(VoidDamageTypes)}::{nameof(ConditionalVoidDeath)})...");
			ConditionalVoidDeath = ReserveDamageType();
		}

	}
}
