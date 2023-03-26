using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.EntityStates {

	/// <summary>
	/// This is a base for the Void Death state. 
	/// <para/>
	/// This type is responsible for preventing the game over screen until <em>after</em> this state is completed, which in the case of Void enemies, occurs after the black hole has imploded.
	/// <para/>
	/// See also: <seealso cref="IHasDelayedGameOver"/>
	/// </summary>
	public class VoidDeathStateBase : GenericCharacterDeath, IHasDelayedGameOver {

		/// <summary>
		/// A reference to the entity's <see cref="CharacterMaster"/>. This is set in the call to <see cref="OnEnter"/>; do not modify this manually unless you explicitly must not invoke the base entry behavior.
		/// </summary>
		protected CharacterMaster Master { get; set; }

		/// <summary>
		/// Prevents automatic destruction.
		/// </summary>
		public sealed override bool shouldAutoDestroy => false;

		/// <inheritdoc/>
		public override void OnEnter() {
			base.OnEnter();
			Master = characterBody.master;
		}

		/// <inheritdoc/>
		public override void OnExit() {
			base.OnExit();
			if (Master == null) {
				string errorType;
				if (Master is null) {
					errorType = $"was never set. The mod implementing this type ({GetType().FullName}) may have forgotten to call base.{nameof(OnEnter)}() in their override to {nameof(OnEnter)}().";
				} else {
					errorType = $"was destroyed by Unity. This should not sensibly happen and represents a potential other serious bug, or another mod doing something that it probably shouldn't be doing. This cannot be avoided by the implementing class.";
				}
				Log.LogError($"{nameof(VoidDeathStateBase)} attempted to perform final cleanup (permitting the game over screen), but this operation is currently not possible as the {nameof(CharacterMaster)} {errorType}");
				Log.LogError("If your game is stuck without a game over screen and you were playing as a Void enemy, this is probably why it happened.");
				return;
			}

			CharacterBody newBody = Master.GetBody();
			if (newBody && newBody.healthComponent.alive) {
				Master.preventGameOver = true;
			} else {
				Master.preventGameOver = false;
			}
		}

	}

	/// <summary>
	/// This interface is what the game actually looks for when seeing if it should prevent the game over screen on death.
	/// <para/>
	/// <see cref="VoidDeathStateBase"/> already implements this, but if (for some reason) you absolutely cannot by any means extend <see cref="VoidDeathStateBase"/>, you should implement this interface manually.
	/// </summary>
	public interface IHasDelayedGameOver { }
}
