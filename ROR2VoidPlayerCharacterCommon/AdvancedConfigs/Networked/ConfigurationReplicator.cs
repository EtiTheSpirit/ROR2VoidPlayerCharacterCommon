using BepInEx.Configuration;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Networking;
using Xan.ROR2VoidPlayerCharacterCommon.ROOInterop;
using static Facepunch.Steamworks.Inventory.Item;
using static UnityEngine.GridBrushBase;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked {

	/// <summary>
	/// This class is responsible for replicating configuration data.
	/// </summary>
	public class ConfigurationReplicator {
		
		/// <summary>
		/// A lookup where keys are mod GUIDs, and values are instances of another lookup where their key is the name of a config entry, and their value is that actual config entry.
		/// </summary>
		internal static readonly Dictionary<string, Dictionary<string, ReplicatedConfigEntryBase>> _entriesByModAndKey = new Dictionary<string, Dictionary<string, ReplicatedConfigEntryBase>>();

		/// <summary>
		/// This is much like <see cref="_entriesByModAndKey"/>, but the secondary lookup (the lookups bound to a mod id) are reversed such that the key is the config instance, and the value is its name.
		/// </summary>
		internal static readonly Dictionary<string, Dictionary<ReplicatedConfigEntryBase, string>> _keysByModAndEntry = new Dictionary<string, Dictionary<ReplicatedConfigEntryBase, string>>();

		/// <summary>
		/// A lookup where keys are mod GUIDs, and values are a lookup from an underlying (local) <see cref="ConfigEntryBase"/> instances to the <see cref="ReplicatedConfigEntryBase"/> that encapsulates it.
		/// </summary>
		internal static readonly Dictionary<string, Dictionary<ConfigEntryBase, ReplicatedConfigEntryBase>> _localToReplicated = new Dictionary<string, Dictionary<ConfigEntryBase, ReplicatedConfigEntryBase>>();

		private static readonly Type[] _primitives = new Type[] {
			typeof(sbyte), typeof(byte),
			typeof(short), typeof(ushort),
			typeof(int), typeof(uint),
			typeof(long), typeof(ulong),
			typeof(float), typeof(double),
			typeof(string)
		};

		/// <summary>
		/// Returns a constant, numeric index of a primitive type, including <see cref="string"/>. The type can be retrieved with <see cref="PrimitiveTypeForIndex(int)"/>.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static int IndexOfPrimitiveType(Type t) {
			for (int i = 0; i < _primitives.Length; i++) {
				if (_primitives[i] == t) return i;
			}
			throw new ArgumentException($"The provided type {t.FullName} is not indexable.");
		}

		/// <summary>
		/// Returns a type for a constant numeric index associated with a type. This index can be acquired with <see cref="IndexOfPrimitiveType(Type)"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Type PrimitiveTypeForIndex(int index) => _primitives[index];

		private readonly string _modId;

		private ConfigurationReplicator(string modId, (ReplicatedConfigEntryBase, string)[] allReplicatedConfigs) {
			_modId = modId;
			for (int i = 0; i < allReplicatedConfigs.Length; i++) {
				(ReplicatedConfigEntryBase cfg, string identity) = allReplicatedConfigs[i];
				if (!_entriesByModAndKey.TryGetValue(modId, out Dictionary<string, ReplicatedConfigEntryBase> modEntries)) {
					modEntries = new Dictionary<string, ReplicatedConfigEntryBase>();
					_entriesByModAndKey[modId] = modEntries;
				}
				if (!_localToReplicated.TryGetValue(modId, out Dictionary<ConfigEntryBase, ReplicatedConfigEntryBase> cfgBindings)) {
					cfgBindings = new Dictionary<ConfigEntryBase, ReplicatedConfigEntryBase>();
					_localToReplicated[modId] = cfgBindings;
				}
				if (!_keysByModAndEntry.TryGetValue(modId, out Dictionary<ReplicatedConfigEntryBase, string> keyBindings)) {
					keyBindings = new Dictionary<ReplicatedConfigEntryBase, string>();
					_keysByModAndEntry[modId] = keyBindings;
				}
				string key = $"{cfg.Key}[{identity}]";
				if (!modEntries.ContainsKey(key)) {
					modEntries[key] = cfg;
					cfgBindings[cfg.BaseLocalBackingConfig] = cfg;
					keyBindings[cfg] = key;
				} else {
					throw new InvalidOperationException($"Duplicate configuration key: {key}");
				}
				
			}

			Run.onRunStartGlobal += TrySendChangesFromRunStart;
		}

		/// <summary>
		/// Disconnects the run start listener.
		/// </summary>
		~ConfigurationReplicator() {
			Run.onRunStartGlobal -= TrySendChangesFromRunStart;
		}

		/// <summary>
		/// Create a configuration replicator. The provided type should be a static class containing all configuration members that are decorated with <see cref="ReplicatedConfigurationAttribute"/>.
		/// </summary>
		/// <param name="modId">The ID of the mod that this is being created for.</param>
		/// <param name="configurationType"></param>
		public static ConfigurationReplicator CreateReplicator(string modId, Type configurationType) {
			
			if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
			if (!configurationType.IsClass) throw new ArgumentException($"The provided type is not a class. Provided type: {configurationType.FullName}", nameof(configurationType));
			if (!configurationType.IsSealed) throw new ArgumentException($"The provided type is not sealed or static. Provided type: {configurationType.FullName}", nameof(configurationType));
			if (configurationType.IsGenericTypeDefinition) throw new ArgumentException($"The configuration type cannot be generic. Provided type: {configurationType.FullName}", nameof(configurationType));

			List<(ReplicatedConfigEntryBase, string)> all = new List<(ReplicatedConfigEntryBase, string)>();
			CollectEntries(modId, configurationType, null, all, configurationType.Name);

			return new ConfigurationReplicator(modId, all.ToArray());
		}

		private static void CollectEntries(string modId, Type type, IReplicatedConfigContainer instance, List<(ReplicatedConfigEntryBase, string)> entries, string parentMemberName) {
			BindingFlags memberFlags = BindingFlags.Public | BindingFlags.NonPublic;
			if (instance != null) {
				memberFlags |= BindingFlags.Instance;
			} else {
				memberFlags |= BindingFlags.Static;
			}
			IEnumerable<MemberInfo> mbrs = type.GetMembers(memberFlags).Where(mbr => mbr.GetCustomAttribute<ReplicatedConfigurationAttribute>() != null);
			foreach (MemberInfo mbr in mbrs) {
				ReplicatedConfigurationAttribute attr = mbr.GetCustomAttribute<ReplicatedConfigurationAttribute>();
				if (mbr is FieldInfo fieldInfo) {
					object val = fieldInfo.GetValue(instance);
					if (val is ReplicatedConfigEntryBase cfgEntry) {
						cfgEntry.IsActuallyNetworked = true;
						entries.Add((cfgEntry, parentMemberName));
						Log.LogTrace($"Found config {mbr.Name} of {type.FullName}.");
					} else if (val is IReplicatedConfigContainer cfgCtr) {
						Log.LogTrace($"Found config container class {mbr.Name} ({cfgCtr.GetType().FullName}) of {type.FullName}.");
						CollectEntries(modId, val.GetType(), cfgCtr, entries, parentMemberName + "$" + mbr.Name);
					} else {
						throw new InvalidOperationException($"Illegal target detected: Member {mbr.Name} of {mbr.DeclaringType.FullName} is not a {nameof(ReplicatedConfigEntryBase)}, and it is not a type that implements {nameof(IReplicatedConfigContainer)} (this may also mean that the value is null). It must be one of these two in order to function.");
					}
				} else if (mbr is PropertyInfo propertyInfo) {
					object val = propertyInfo.GetValue(instance);
					if (val is ReplicatedConfigEntryBase cfgEntry) {
						cfgEntry.IsActuallyNetworked = true;
						entries.Add((cfgEntry, parentMemberName));
						Log.LogTrace($"Found config {mbr.Name} of {type.FullName}.");
					} else if (val is IReplicatedConfigContainer cfgCtr) {
						Log.LogTrace($"Found config container class {mbr.Name} ({cfgCtr.GetType().FullName}) of {type.FullName}.");
						CollectEntries(modId, val.GetType(), cfgCtr, entries, parentMemberName + "$" + mbr.Name);
					} else {
						throw new InvalidOperationException($"Illegal target detected: Member {mbr.Name} of {mbr.DeclaringType.FullName} is not a {nameof(ReplicatedConfigEntryBase)}, and it is not a type that implements {nameof(IReplicatedConfigContainer)} (this may also mean that the value is null). It must be one of these two in order to function.");
					}
				} else {
					throw new InvalidOperationException($"Illegal target detected: Member {mbr.Name} of {mbr.DeclaringType.FullName} is not a field or property; it cannot use {nameof(ReplicatedConfigurationAttribute)}.");
				}
			}
		}

		/// <summary>
		/// Attempts to send the changes across the network, returning if the act was successful. This only works on the host's machine. Note that this method is automatically called by an <see cref="AdvancedConfigBuilder"/>.
		/// </summary>
		/// <returns></returns>
		public bool TrySendChanges(SettingChangedEventArgs e) {
			if (!NetworkServer.active) return false;
			if (e != null) {
				if (_localToReplicated.TryGetValue(_modId, out Dictionary<ConfigEntryBase, ReplicatedConfigEntryBase> bepSettingToReplicated)) {
					ConfigEntryBase toFind = e.ChangedSetting;
					if (toFind != null && bepSettingToReplicated.TryGetValue(toFind, out ReplicatedConfigEntryBase replicatedCfg)) {
						Log.LogTrace("Sending single config.");
						string key = _keysByModAndEntry[_modId][replicatedCfg];
						new ConfigurationReplicationMessage(_modId, key, replicatedCfg).Send(R2API.Networking.NetworkDestination.Clients);
						return true;
					} else if (toFind != null) {
						Log.LogWarning($"Failed to find a replicated configuration entry for option \"{toFind.Definition.Key}\". To avoid a desynchronization, all config data will be sent over at once.");
					}
				} else {
					return false; // If it's not in the list, it's not a replicated change. Do not try anything.
				}
			}
			Log.LogTrace("Sending all data.");
			foreach (KeyValuePair<string, ReplicatedConfigEntryBase> data in _entriesByModAndKey[_modId]) {
				new ConfigurationReplicationMessage(_modId, data.Key, data.Value).Send(R2API.Networking.NetworkDestination.Clients);
			}
			return true;
		}

		internal static bool TrySendChangesStatic(string modid) {
			if (!NetworkServer.active) return false;
			Log.LogTrace("Sending all data (as per a request).");
			foreach (KeyValuePair<string, ReplicatedConfigEntryBase> data in _entriesByModAndKey[modid]) {
				new ConfigurationReplicationMessage(modid, data.Key, data.Value).Send(R2API.Networking.NetworkDestination.Clients);
			}
			return true;
		}

		private void TrySendChangesFromRunStart(Run run) {
			Log.LogDebug("A run started...");
			if (!TrySendChanges(null)) {
				// Maybe not the server, are we a client
				Log.LogDebug("Requesting data.");
				new ConfigurationRequestMessage(_modId).Send(R2API.Networking.NetworkDestination.Server);
			}
		}

		internal class ConfigurationReplicationMessage : INetMessage {
			public ConfigurationReplicationMessage() { }

			private string _modId;
			private string _entryKey;
			private ReplicatedConfigEntryBase _replicatedValue;

			public void OnReceived() { }

			public void Serialize(NetworkWriter writer) {
				writer.Write(_modId);
				writer.Write(_entryKey);
				string toml = TomlTypeConverter.ConvertToString(_replicatedValue.BoxedLocalValue, _replicatedValue.Type);
				writer.Write(toml);
				Log.LogTrace($"Sent config for [{_modId}]: {_entryKey} => {toml}");
			}

			public void Deserialize(NetworkReader reader) {
				_modId = reader.ReadString();
				_entryKey = reader.ReadString();
				string toml = reader.ReadString();
				_replicatedValue = _entriesByModAndKey[_modId][_entryKey];
				object realValue = TomlTypeConverter.ConvertToValue(toml, _replicatedValue.Type);
				_replicatedValue.BoxedReplicatedValue = realValue;
				Log.LogTrace($"Received config for [{_modId}]: {_entryKey} => {toml} ({realValue})");
			}

			public ConfigurationReplicationMessage(string modId, string entryKey, ReplicatedConfigEntryBase replicatedValue) {
				_modId = modId;
				_entryKey = entryKey;
				_replicatedValue = replicatedValue;
			}
		}
		internal class ConfigurationRequestMessage : INetMessage {
			private string _modId;

			public ConfigurationRequestMessage() { }

			public ConfigurationRequestMessage(string modId) {
				_modId = modId;
			}

			public void OnReceived() {
				TrySendChangesStatic(_modId);
			}

			public void Serialize(NetworkWriter writer) {
				writer.Write(_modId);
			}

			public void Deserialize(NetworkReader reader) {
				_modId = reader.ReadString();
			}
		}
	}
	
}
