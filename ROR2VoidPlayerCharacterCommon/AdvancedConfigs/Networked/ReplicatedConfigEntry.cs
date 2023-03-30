using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked {

	/// <summary>
	/// The base class for a replicated config entry.
	/// </summary>
	public abstract class ReplicatedConfigEntryBase {
		
		/// <summary>
		/// Because this will always (sensibly) be an instance of <see cref="ReplicatedConfigEntry{T}"/>, this is the type of its type parameter.
		/// </summary>
		public virtual Type Type { get; }

		/// <summary>
		/// The replicated value represented as an object.
		/// </summary>
		public abstract object BoxedReplicatedValue { get; internal set; }

		/// <summary>
		/// The local value represented as an object.
		/// </summary>
		public abstract object BoxedLocalValue { get; set; }

		/// <summary>
		/// The key of this configuration option.
		/// </summary>
		public abstract string Key { get; }

		/// <summary>
		/// The section that this entry is in.
		/// </summary>
		public abstract string Section { get; }

		/// <summary>
		/// A reference to the local value's <see cref="ConfigEntryBase"/>.
		/// </summary>
		public abstract ConfigEntryBase BaseLocalBackingConfig { get; }

		/// <summary>
		/// Whether or not this is a networked config entry. If false, this is simply serving as a box for a local config. 
		/// This will remain false until it is initialized by a <see cref="ConfigurationReplicator"/>.
		/// </summary>
		public virtual bool IsActuallyNetworked { get; internal set; }

	}

	/// <summary>
	/// This is much like a <see cref="ConfigEntry{T}"/> but it has a second value for the one received over the network. <strong>This may not necessarily be networked, depending on the presence of a <see cref="ReplicatedConfigurationAttribute"/></strong>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReplicatedConfigEntry<T> : ReplicatedConfigEntryBase, IDisposable {

		/// <summary>
		/// The value as mandated by the network master. This may not be the proper value. Use <see cref="Value"/> to get the proper value.
		/// </summary>
		public T ReplicatedValue {
			get => _replicatedValue;
			internal set {
				if (Equals(_replicatedValue, value)) return;
				_replicatedValue = value;
				SettingChanged?.Invoke(value, true);
			}
		}
		private T _replicatedValue;

		/// <summary>
		/// The value stored on this machine's configs. This may not be the proper value. Use <see cref="Value"/> to get the proper value.
		/// </summary>
		public T LocalValue {
			get => LocalBackingConfig.Value;
			set {
				if (Equals(LocalBackingConfig.Value, value)) return;
				_isChangingFromSetter = true;
				LocalBackingConfig.Value = value;
				_isChangingFromSetter = false;
				SettingChanged?.Invoke(value, Run.instance == null || NetworkServer.active);
			}
		}

		/// <inheritdoc/>
		public override Type Type { get; }

		/// <inheritdoc/>
		public override string Key => LocalBackingConfig.Definition.Key;

		/// <inheritdoc/>
		public override string Section => LocalBackingConfig.Definition.Section;

		/// <inheritdoc/>
		public override object BoxedReplicatedValue {
			get => ReplicatedValue;
			internal set => ReplicatedValue = (T)value;
			
		}

		/// <inheritdoc/>
		public override object BoxedLocalValue {
			get => LocalValue;
			set => LocalValue = (T)value;
		}

		/// <summary>
		/// The current effective value that the game should be using. 
		/// This will be <see cref="LocalValue"/> if a run is not live or if this machine is the host (including in singleplayer), and <see cref="ReplicatedValue"/> if this player is in multiplayer and is not the host.
		/// </summary>
		public T Value => (Run.instance == null || NetworkServer.active) ? LocalValue : ReplicatedValue;

		/// <summary>
		/// A reference to the local value's <see cref="ConfigEntry{T}"/>. The implicit cast to <see cref="ConfigEntry{T}"/> returns this.
		/// </summary>
		public ConfigEntry<T> LocalBackingConfig { get; }

		/// <inheritdoc/>
		public override ConfigEntryBase BaseLocalBackingConfig => LocalBackingConfig;

		/// <summary>
		/// Wrap a <see cref="ConfigEntry{T}"/> so that it can be replicated.
		/// </summary>
		/// <param name="backingConfig"></param>
		public ReplicatedConfigEntry(ConfigEntry<T> backingConfig) {
			LocalBackingConfig = backingConfig;
			Type = typeof(T);
			backingConfig.SettingChanged += OnBackingCfgChanged;
		}

		private bool _isChangingFromSetter = false;
		private void OnBackingCfgChanged(object sender, EventArgs e) {
			if (_isChangingFromSetter) return;
			SettingChangedEventArgs evt = (SettingChangedEventArgs)e;
			if (evt.ChangedSetting == LocalBackingConfig) {
				SettingChanged?.Invoke(Value, Run.instance == null || NetworkServer.active);
			}
		}

		/// <summary>
		/// Returns a reference to the <strong>locally stored</strong> <see cref="ConfigEntry{T}"/> within this replicated entry.
		/// </summary>
		/// <param name="replicated"></param>
		public static implicit operator ConfigEntry<T>(ReplicatedConfigEntry<T> replicated) {
			return replicated.LocalBackingConfig;
		}

		/// <summary>
		/// This event fires when both the local and remote value changes. The delegate offers two parameters. See <see cref="ChangeOccurredDelegate"/> for more information.
		/// </summary>
		public event ChangeOccurredDelegate SettingChanged;

		/// <summary>
		/// This delegate is used for <see cref="SettingChanged"/>.
		/// </summary>
		/// <param name="newValue">The new value that this was set to.</param>
		/// <param name="fromHost">If this is true, three possible scenarios have occurred. #1: This was received over the network from the host. #2: This was a local change, but the local user is the host of the current ongoing run. #3: This was a local change made outside of a run. This value is good for determining if a change to a system should actually be performed.</param>
		public delegate void ChangeOccurredDelegate(T newValue, bool fromHost);

		/// <summary>
		/// Disconnects a handler for when <see cref="LocalBackingConfig"/> changes.
		/// </summary>
		public void Dispose() {
			try {
				LocalBackingConfig.SettingChanged -= OnBackingCfgChanged;
			} catch (Exception exc) {
				Log.LogError($"Failed to disconnect SettingChanged: {exc}");
			}
		}
	}
}
