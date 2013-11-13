using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Attribute which marks the root asset class of an asset.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class RootAssetAttribute : Attribute
	{
		/// <summary>
		/// The friendly name of the asset (less than 30 characters, may have spaces, other punctuation).
		/// </summary>
		public string Name;
		/// <summary>
		/// The C style id for the asset (must be a valid reasonable C identifier, preferrably with no upper case, underscore as spaces).
		/// </summary>
		public string Id;
		/// <summary>
		/// The file extension of the asset in '.ext' format.
		/// </summary>
		public string Extension;
		/// <summary>
		/// Is this asset static (immutable) data when used at runtime.
		/// </summary>
		public bool Static;

		public RootAssetAttribute ()
		{
		}
	}
}

