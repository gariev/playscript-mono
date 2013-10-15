using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Interface implemented by all objects in an asset tree in both -tooling:game and -tooling:editor mode.
	/// </summary>
	public interface IAssetObject
	{
		/// <summary>
		/// The pointer to the binary backing instance data for this object (initial data for instance of this object).
		/// </summary>
		/// <value>The data.</value>
		IntPtr Data { get; }

		/// <summary>
		/// The pointer to the binary backing config data for this object (common data for all objects of this type).
		/// </summary>
		/// <value>The config data.</value>
		IntPtr ConfigData { get; }

		/// <summary>
		/// Gets the asset pool this asset object's data is stored in.
		/// </summary>
		/// <value>The asset pool.</value>
		AssetPool AssetPool { get; }
	}
}

