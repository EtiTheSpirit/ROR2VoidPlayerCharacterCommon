using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs.Networked {

	/// <summary>
	/// If you make a custom config container class (such as <see cref="ReplicatedVector3Wrapper"/>, for example), it should implement this interface to work with the <see cref="ReplicatedConfigurationAttribute"/>.
	/// </summary>
	public interface IReplicatedConfigContainer { }
}
