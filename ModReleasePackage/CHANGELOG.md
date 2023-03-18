# 2.0.0
* New submodules: Replicated Configuration, Advanced Configuration. **You can now edit almost every stat without having to restart the game. This goes so far as to permit some stats to be edited in the middle of a run!** Alongside this, stats are network-enforced, so in multiplayer environments the host's settings are automatically sent over! No more accidentally desynchronized configs.
* The Survivor Hollower system (used to translate enemies into players during the game loading phase) has been moved to this system, and thus removed from both Reaver and Jailer.
* Added Devastator Bomblet as a conditional void projectile
* Added partial void death VFX (do damage, but if it's a kill, spawn the void on-kill effect. This is what Reaver's "Detain" special did).
* Removed Manual Void Death, this became obsolete and ended up just being normal void death with extra steps.
* Jailer's VR interop system has been moved to this mod (the Devastator will likely make use of it as well, but reaver will not).

# 1.1.1
* Draw attention to the configuration options in the readme.
* Fix a wording mistake in the configs that implied the Black Hole settings applied to *all* void enemies. These settings only apply to players.

# 1.1.0
* Make some members internal
* Add a bunch of missing members. Probably should have not uploaded this yet. Oops.
* Added the option (on by default) to disable the Low Pass Filter after dying when the death state is known to be a manually defined Void death.
* Added the option (on by default) to register all known vanilla void enemies for scripted immunity to fog. This prevents weird edge cases with some void spawns taking damage in their own atmosphere.
* Added the option to globally configure the mechanics of player-created void black holes. The options are the same as the original Reaver mod, where instakills to monsters can be toggled, instakills to bosses can be toggled, and friendly fire can be toggled. The fallback damage (when an instakill isn't possible) is also a configurable value.

# 1.0.0
* Initial release of the mod.