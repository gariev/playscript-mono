using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Interface implemented by all objects in an asset tree in both -tooling:game and -tooling:editor mode.
	/// </summary>
	public interface IAssetObject
	{
		/// <summary>
		/// The pointer to the binary backing config data for this object (common data for all objects of this type).
		/// </summary>
		/// <value>The config data.</value>
		IntPtr Data { get; }

	}
}

