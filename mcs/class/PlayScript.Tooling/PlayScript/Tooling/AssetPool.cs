using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
		// The block definition for the top level catalog struct in an asset pool buffer.
		private static BlockDef _poolCatalogDef;

		private struct PoolBufferSpan
		{
			public ulong Start;
			public ulong End;
		}

		// Array of buffer spans that are checked when freeing memory (we don't free memory that's inside a packed buffer)
		private static List<PoolBufferSpan> _spans = new List<PoolBufferSpan>();

		private string _path;
		private byte[] _buffer;
		GCHandle _gcHandle;
		private IntPtr _bufferPtr;
		private int _size;

		static AssetPool() 
		{
			_poolCatalogDef = new BlockDef ();
			_poolCatalogDef.Name = "AssetPoolCatalog";
			_poolCatalogDef.AddField (new FieldDef { Name =  "assets", FieldType = FieldType.Array });
			Schema.TopSchema.AddBlock (_poolCatalogDef);
		}

		public AssetPool ()
		{
		}

		public static BlockDef PoolCatalogBlock {
			get {
				return _poolCatalogDef;
			}
		}

		public string Path { 
			get {
				return _path;
			}
			set {
				_path = value;
			}
		}

		private void FreeBuffer()
		{
			if (_buffer != null) {
				if (_gcHandle != null)
					_gcHandle.Free ();
				int spanPos = 0;
				ulong start = (ulong)_bufferPtr.ToInt64 ();
				while (spanPos < _spans.Count) {
					if (_spans [spanPos].Start = start) {
						break;
					}
				}
				_spans.RemoveAt (spanPos);
				_gcHandle = null;
				_buffer = null;
				_bufferPtr = null;
				_size = 0;
			}
		}

		/// <summary>
		/// Load a new pool from the given path asynchronously, calls callback when complete.
		/// </summary>
		/// <param name="path">The path.</param>
		public static void Load(string path, Action<AssetPool, bool> callback) 
		{
			if (_buffer != null)
				FreeBuffer ();
			_path = path;
			_buffer = System.IO.File.ReadAllBytes (path);
			_gcHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
			_bufferPtr = _gcHandle.AddrOfPinnedObject();
			_poolCatalogDef.FixupPointers (_bufferPtr);
			ulong start = (ulong)_bufferPtr.ToInt64 ();
			_size = _buffer.Length;
			_spans.Add (new PoolBufferSpan { Start = start, End = start + _size }); 
			callback (this, true);
		}

		/// <summary>
		/// Saves this asset pool to storage.  Pool is stored in the format specified by the file extension.
		/// </summary>
		public void Save(Action<AssetPool, bool> callback)
		{
			SaveAs (_path, callback);
		}

		/// <summary>
		/// Saves this asset pool to storage.  Pool is stored in the format specified by the file extension.
		/// </summary>
		/// <param name="path">Path.</param>
		public void SaveAs(string path, Action<AssetPool, bool> callback)
		{
			if (_buffer == null)
				throw new InvalidOperationException ("The asset pool is empty.");
			_poolCatalogDef.UnfixupPointers (_bufferPtr);
			System.IO.File.WriteAllBytes (path, _buffer);
			_poolCatalogDef.FixupPointers (_bufferPtr);
			callback (this, true);
		}

		/// <summary>
		/// Tries to get an asset from the asset pool given an asset name.
		/// </summary>
		/// <returns><c>true</c>, if get asset was tryed, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		/// <param name="ptr">Ptr.</param>
		/// <param name="blockDef">Block def.</param>
		public bool TryGetAsset(string name, out IntPtr ptr, out BlockDef blockDef)
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

		/// <summary>
		/// Returns a new buffer associated with this pool.
		/// </summary>
		/// <returns>The buffer.</returns>
		/// <param name="len">Length.</param>
		public static IntPtr AllocBuffer(int len) 
		{
			return System.Runtime.InteropServices.Marshal.AllocHGlobal(len);
		}

		/// <summary>
		/// Frees a memory buffer or string.
		/// </summary>
		/// <param name="buffer">Buffer to free.</param>
		public static void FreeBuffer(IntPtr buffer) 
		{
			ulong ptr = (ulong)buffer.ToInt64 ();
			int len = _spans.Count;
			for (int i = 0; i < len; i++) {
				if (ptr >= _spans [i].Start && ptr < _spans [i].End) {
					return; // Don't free me!  I'm embedded in a pool buffer.
				}
			}
			System.Runtime.InteropServices.Marshal.FreeHGlobal (buffer);
		}

		/// <summary>
		/// Allocs a new UTF-8 string associated with this pool.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="s">The string.</param>
		public static IntPtr AllocString(string s) 
		{
			if (s == null)
				return new IntPtr ((void*)0);
			// TODO: Check to see if we need to do UTF-8 encoding.. 
			return System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi (s);
		}

		/// <summary>
		/// Allocs a new UTF-8 string associated with this pool.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="s">The string.</param>
		public static IntPtr CloneString(IntPtr s) 
		{
			byte* sptr = s.ToPointer ();
			if (sptr == (byte*)0)
				return new IntPtr ((void*)0);

			int len = 0;
			byte* p = sptr;
			while (*p != 0) {
				p++;
				len++;
			}

			IntPtr newStr = System.Runtime.InteropServices.Marshal.AllocHGlobal (len + 1);

			p = sptr;
			byte* d = newStr.ToPointer();
			while (*p != 0) {
				*d++ = *p++;
			}
			*d++ = 0;

			return newStr;
		}

		/// <summary>
		/// Allocates an array of a given size.
		/// </summary>
		/// <param name="arrayPtr">Array ptr.</param>
		public static void AllocArray(IntPtr arrayPtr, BlockDef blockDef, int count) 
		{
		}

	}
}

