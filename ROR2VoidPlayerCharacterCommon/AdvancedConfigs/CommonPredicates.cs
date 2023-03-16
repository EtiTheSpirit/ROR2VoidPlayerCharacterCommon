using System;
using System.Collections.Generic;
using System.Text;

namespace Xan.ROR2VoidPlayerCharacterCommon.AdvancedConfigs {
	
	/// <summary>
	/// Contains predicates for use in ROO.
	/// </summary>
	public static class CommonPredicates {

		/// <summary>
		/// This returns true outside of a run
		/// </summary>
		[Obsolete("This has not been implemented yet and will do nothing.", true)]
		public static readonly Func<bool> HostOnly = () => true; // TODO

	}
}
