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
		/// This damage type should only be applied when the damage should (and would not normally) have the <em>visual appearance</em> of <see cref="DamageType.VoidDeath"/>.
		/// <para/>
		/// <strong>Do not</strong> add this damage type if its <see cref="DamageInfo"/> either:<br/>
		/// 1: Is classified as <see cref="DamageType.VoidDeath"/> (this damage type already performs the effect), or<br/>
		/// 2: Is classified as <see cref="BlacklistExaggeratedVoidDeath"/> (it should never actually display)
		/// </summary>
		public static ModdedDamageType DisplayVoidDeathOnKill { get; private set; }

		/// <summary>
		/// This damage type can be applied to tell the Exaggerated Void Death mod's effects to not apply. This should strictly be used iff the incoming damage type has <see cref="DamageType.VoidDeath"/>.
		/// If the damage type has <see cref="DisplayVoidDeathOnKill"/> and you use this, you are literally applying the thing and then canceling it out. Just remove it 16384head.
		/// </summary>
		public static ModdedDamageType BlacklistExaggeratedVoidDeath { get; private set; }

		/// <summary>
		/// This damage type should only be applied to a <see cref="DamageInfo"/> whose type already includes <see cref="DamageType.VoidDeath"/>. This makes the death "conditional" in that it will use
		/// <see cref="XanVoidAPI.CanCharacterInstakillMonsters(BodyIndex)"/> (and the related methods) to figure out whether or not it should actually apply as <see cref="DamageType.VoidDeath"/>.
		/// </summary>
		public static ModdedDamageType ConditionalVoidDeath { get; private set; }

		/// <summary>
		/// This damage type is an extension to <see cref="ConditionalVoidDeath"/> that enforces an instakill can never actually happen, meaning the fallback damage
		/// (see <see cref="XanVoidAPI.GetFallbackDamage(BodyIndex)"/>) is always used no matter what. 
		/// Naturally this is only useful on damage types that are otherwise capable of causing <see cref="DamageType.VoidDeath"/>.
		/// </summary>
		public static ModdedDamageType NeverVoidDeath { get; private set; }

		/// <summary>
		/// This is useful if fog needs to damage void characters anyway, such as through some ability. Applying this damage type to a <see cref="DamageInfo"/> matching the characteristics of Void Fog
		/// will make it able to damage void characters, even if they resist fog. <strong>This does not affect visual fog, only the damage that is applied.</strong>
		/// </summary>
		public static ModdedDamageType BypassFogResistance { get; private set; }

		/// <summary>
		/// Designed explicitly for the Exaggerated Void Deaths mod, this tells the system to make the void kill sound anyway instead of doing what it normally does and silencing it.
		/// </summary>
		public static ModdedDamageType ExaggeratedVoidDeathRequiresNoise { get; private set; }

		internal static void Initialize() {
			Log.LogInfo("Initializing Void Damage Types...");
			Log.LogTrace($"Visual Void Death ({nameof(VoidDamageTypes)}::{nameof(DisplayVoidDeathOnKill)})...");
			DisplayVoidDeathOnKill = ReserveDamageType();
			Log.LogTrace($"Exaggerated Void Death Blacklist ({nameof(VoidDamageTypes)}::{nameof(BlacklistExaggeratedVoidDeath)})...");
			BlacklistExaggeratedVoidDeath = ReserveDamageType();
			Log.LogTrace($"Conditional Void Death ({nameof(VoidDamageTypes)}::{nameof(ConditionalVoidDeath)})...");
			ConditionalVoidDeath = ReserveDamageType();
			Log.LogTrace($"Never Void Death ({nameof(VoidDamageTypes)}::{nameof(NeverVoidDeath)})...");
			NeverVoidDeath = ReserveDamageType();
			Log.LogTrace($"Bypass Fog Resistance ({nameof(VoidDamageTypes)}::{nameof(BypassFogResistance)})...");
			BypassFogResistance = ReserveDamageType();
			Log.LogTrace($"Exaggerated Void Death Requires Noise ({nameof(VoidDamageTypes)}::{nameof(ExaggeratedVoidDeathRequiresNoise)})...");
			ExaggeratedVoidDeathRequiresNoise = ReserveDamageType();
		}

	}
}
