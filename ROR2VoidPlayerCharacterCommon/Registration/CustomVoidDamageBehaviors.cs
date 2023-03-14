using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.Registration {

	/// <summary>
	/// Handles the registration of how to handle Void damage.
	/// </summary>
	internal static class CustomVoidDamageBehaviors {

		private static readonly Dictionary<BodyIndex, ConfigProxy> _settings = new Dictionary<BodyIndex, ConfigProxy>();
		private static readonly Dictionary<BodyIndex, BaseUnityPlugin> _owners = new Dictionary<BodyIndex, BaseUnityPlugin>();

		internal static void RegisterConfigProxy(BaseUnityPlugin registrar, BodyIndex bodyIndex, ConfigEntry<bool> useModSettings, ConfigEntry<bool> allowInstakillMonsters, ConfigEntry<bool> allowInstakillBosses, ConfigEntry<bool> allowFriendlyFire, ConfigEntry<float> fallbackDamage) {
			if (registrar == null) {
				throw new ArgumentNullException(nameof(registrar));
			}
			if (!BodyCatalog.availability.available) {
				throw new InvalidOperationException("Please wait until the body catalog has finished loading. You can do this with BodyCatalog.availability.onAvailable");
			}
			if (_settings.ContainsKey(bodyIndex)) {
				throw new ArgumentException($"The provided body ({Helpers.BodyToString(bodyIndex)}) has already been registered by {Helpers.ModToString(_owners[bodyIndex])}.");
			}
			ConfigProxy proxy = new ConfigProxy(useModSettings, allowInstakillMonsters, allowInstakillBosses, allowFriendlyFire, fallbackDamage);
			_settings[bodyIndex] = proxy;
			_owners[bodyIndex] = registrar;
		}

		internal static bool CanCharacterInstakillMonsters(BodyIndex bodyIndex) {
			_settings.TryGetValue(bodyIndex, out ConfigProxy proxy);
			return (proxy ?? ConfigProxy.DEFAULT).CanBlackHoleInstakillMonsters;
		}

		internal static bool CanCharacterInstakillBosses(BodyIndex bodyIndex) {
			_settings.TryGetValue(bodyIndex, out ConfigProxy proxy);
			return (proxy ?? ConfigProxy.DEFAULT).CanBlackHoleInstakillBosses;
		}

		internal static bool CanCharacterFriendlyFire(BodyIndex bodyIndex) {
			_settings.TryGetValue(bodyIndex, out ConfigProxy proxy);
			return (proxy ?? ConfigProxy.DEFAULT).CanBlackHoleFriendlyFire;
		}

		internal static float GetFallbackDamage(BodyIndex bodyIndex) {
			_settings.TryGetValue(bodyIndex, out ConfigProxy proxy);
			return (proxy ?? ConfigProxy.DEFAULT).FallbackDamage;
		}

		private class ConfigProxy {

			public static readonly ConfigProxy DEFAULT = new ConfigProxy();

			private T Get<T>(ConfigEntry<T> modProvided, T globalDefault) where T : struct {
				if (ReferenceEquals(this, DEFAULT)) return globalDefault;

				bool useModSettings = _useModSettings?.Value ?? true;
				if (!useModSettings) return globalDefault;
				return modProvided?.Value ?? globalDefault;
			}

			public bool CanBlackHoleInstakillMonsters => Get(_allowInstakillMonsters, Configuration.AllowPlayerBlackHoleToInstakill);

			public bool CanBlackHoleInstakillBosses => Get(_allowInstakillBosses, Configuration.AllowPlayerBlackHoleToInstakillBosses);

			public bool CanBlackHoleFriendlyFire => Get(_allowFriendlyFire, Configuration.BlackHoleFriendlyFire);

			public float FallbackDamage => Get(_fallbackDamage, Configuration.BlackHoleBackupDamage);

			private ConfigEntry<bool> _useModSettings;

			private ConfigEntry<bool> _allowInstakillMonsters;

			private ConfigEntry<bool> _allowInstakillBosses;

			private ConfigEntry<bool> _allowFriendlyFire;

			private ConfigEntry<float> _fallbackDamage;

			public ConfigProxy(ConfigEntry<bool> useModSettings, ConfigEntry<bool> allowInstakillMonsters, ConfigEntry<bool> allowInstakillBosses, ConfigEntry<bool> allowFriendlyFire, ConfigEntry<float> fallbackDamage) {
				this._useModSettings = useModSettings;
				this._allowInstakillMonsters = allowInstakillMonsters;
				this._allowInstakillBosses = allowInstakillBosses;
				this._allowFriendlyFire = allowFriendlyFire;
				this._fallbackDamage = fallbackDamage;
			}

			internal ConfigProxy() { }

		}
	}
}
