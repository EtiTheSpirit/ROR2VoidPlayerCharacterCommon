using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked {
	
	/// <summary>
	/// Decorate any <see cref="ConfigEntryBase"/> or <see cref="IReplicatedConfigContainer"/> with this attribute to replicate it automatically when configs are changed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class ReplicatedConfigurationAttribute : Attribute {
		
		/// <summary>
		/// Construct a new <see cref="ReplicatedConfigurationAttribute"/> to mark a config entry as replicated.
		/// </summary>
		public ReplicatedConfigurationAttribute() { }

	}

	/// <summary>
	/// Purely cosmetic. This serves as a programmatical reminder to not use <see cref="ReplicatedConfigurationAttribute"/>.
	/// <strong>This attribute is implicit.</strong> Not having <see cref="ReplicatedConfigurationAttribute"/> implicitly means this attribute is on it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class DoNotReplicateAttribute : Attribute { }
}
