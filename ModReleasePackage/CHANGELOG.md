# 1.2.0
* Add Devastator Bomblet as a conditional void projectile
* Add partial void death VFX (do damage, but if it's a kill, spawn the void on-kill effect).
* Remove Manual Void Death, this became obsolete and ended up just being normal void death with extra steps.
* Add ROO dependency and an Advanced Configuration module. This module allows config options to be declared a *little* cleaner and to be edited in-game, at the sacrifice of making the config file a little messier.
* Jailer's VR interop system has been moved to this mod (the Devastator will likely make use of it).
* The Survivor Hollower system (used to translate enemies into players during the game loading phase) has been moved to this system, and thus removed from both Reaver and Jailer.

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