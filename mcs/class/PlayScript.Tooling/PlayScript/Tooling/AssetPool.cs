using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Implements a binary asset pool in which the initial values of asset objects are stored. 
	/// </summary>
	/// <remarks>
	/// <para>An asset pool is essentially a binary buffer which stores the initial values of the assets contained in it.  The asset pool can 
	/// be loaded by directly deserializing the binary data from a file, or by reading the data from a collection of JSON, XML or other
	/// standardized files.   
	/// </para>
	/// <para>
	/// The binary version of the asset pool is considered to be a "cache" or "distribution" format and should not be used to store the original
	/// data for any assets.  Assets source data should be stored in a different file which is self describing such as JSON, AMF, etc.
	/// </para>
	/// </remarks>
	public unsafe class AssetPool
	{
		// The path of the asset
		public string _path;

		// The buffer that stores the compacted asset data
		public byte* _buffer; 

		public AssetPool (string path)
		{
			_path = path;
		}

		public string Path { 
			get {
				return _path;
			}
			set {
				_path = value;
			}
		}

		public static AssetPool Load(string path) 
		{
			return null;
		}

		/// <summary>
		/// Saves this asset pool to storage.  Pool is stored in the format specified by the file extension.
		/// </summary>
		public void Save()
		{
			SaveAs (_path);
		}

		/// <summary>
		/// Saves this asset pool to storage.  Pool is stored in the format specified by the file extension.
		/// </summary>
		/// <param name="path">Path.</param>
		public void SaveAs(string path)
		{
		}

		/// <summary>
		/// Compacts this asset pool into a single contiguous byte buffer.
		/// </summary>
		public void Compact()
		{
		}

		/// <summary>
		/// Merges this asset pool with another asset pool.
		/// </summary>
		/// <param name="assetPool">Asset pool.</param>
		public void Merge(AssetPool assetPool) 
		{
		}

		/// <summary>
		/// Creates a new instance of an asset given an id.
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="id">Identifier.</param>
		public T CreateInstance<T>(string id) where T : class, IAssetObject
		{
			return null;
		}

		/// <summary>
		/// Adds a new asset of the given type with the given id.
		/// </summary>
		/// <returns>The new asset.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public object AddAsset<T> (string id) where T : class, IAssetObject 
		{
			return null;
		}
	}
}

