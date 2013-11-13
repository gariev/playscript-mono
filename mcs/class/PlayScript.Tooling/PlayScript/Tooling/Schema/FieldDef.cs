using System;
using System.Runtime.InteropServices;

namespace PlayScript.Tooling
{
	[StructLayout(LayoutKind.Explicit, Size=12)]  // Accomodate 32/64 bit pointers
	internal unsafe struct BlockElement
	{
		[FieldOffset(0)]
		public uint CrcId;
		[FieldOffset(4)]
		public void* Ptr;
	}

	[StructLayout(LayoutKind.Explicit, Size=12)]  // Accomodate 32/64 bit pointers
	internal unsafe struct SparseElement
	{
		[FieldOffset(0)]
		public uint CrcId;
		[FieldOffset(4)]
		public ulong Data;
	}

	public class FieldDef
	{
		internal BlockDef _parent;
		internal Schema _schema;

		private bool _frozen;
		private string _name;
		private uint _crcId;
		private FieldType _fieldType;
		private FieldType _elementType;
		private string _blockDefName;
		internal BlockDef _blockDef;
		private object _defaultValue;
		private int _start;
		private int _length;
		private bool _isSparse;
		private bool _isName;

		public FieldDef ()
		{
		}

		private void CheckFrozen() {
			if (_frozen) 
				throw new InvalidOperationException ("Data is read only.");
		}

		public void Freeze()
		{
			_frozen = true;
		}

		public BlockDef Parent {
			get { return _parent; }
		}

		public Schema Schema {
			get { return _schema; }
		}

		public string Name {
			get { return _name; }
			set {
				CheckFrozen ();
				_name = value;
				_crcId = Crc32.Calculate (_name);
			}
		}

		public int CrcId {
			get { return _crcId; }
		}

		public FieldType FieldType {
			get { return _fieldType; }
			set {
				CheckFrozen ();
				_fieldType = value; 
			}
		}

		public FieldType ElementType {
			get { return _elementType; }
			set {
				CheckFrozen ();
				_elementType = value; 
			}
		}

		public string BlockDefName {
			get { return _blockDefName; }
			set {
				CheckFrozen ();
				_blockDefName = value;
				_blockDef = null;
				if (_schema != null) {
					int index = _schema.GetBlockIndex (_blockDefName);
					if (index != -1)
						_blockDef = _schema.GetBlockAt (index);
				}
			}
		}

		public BlockDef BlockDef {
			get { return _blockDef; }
			set {
				CheckFrozen ();
				_blockDef = value; 
				if (_blockDef != null)
					_blockDefName = _blockDef.Name;
				else
					_blockDefName = null;
			}
		}

		public object DefaultValue {
			get { return _defaultValue; }
			set {
				CheckFrozen ();
				_defaultValue = value; 
			}
		}

		public int Start {
			get { return _start; }
			set {
				CheckFrozen ();
				_start = value; 
			}
		}

		public int Length {
			get { return _length; }
			set {
				CheckFrozen ();
				_length = value; 
			}
		}

		public bool IsSparse {
			get { return _isSparse; }
			set {
				CheckFrozen ();
				_isSparse = value; 
			}
		}

		public bool IsName {
			get { return _isName; }
			set {
				CheckFrozen ();
				_isName = value; 
			}
		}

		public void GetDefaultValue(ref Value value)
		{
			// Get value
			switch (_fieldType) {
			case FieldType.Bool:
				value._bool = _defaultValue != null ? (bool)_defaultValue : false;
				value._object = typeof(bool);
				break;
			case FieldType.Byte:
				value._byte = _defaultValue != null ? (byte)_defaultValue : (byte)0;
				value._object = typeof(byte);
				break;
			case FieldType.SByte:
				value._sbyte = _defaultValue != null ? (sbyte)_defaultValue : (sbyte)0;
				value._object = typeof(sbyte);
				break;
			case FieldType.Char:
				value._char = _defaultValue != null ? (char)_defaultValue : '\x0';
				value._object = typeof(char);
				break;
			case FieldType.Short:
				value._short = _defaultValue != null ? (short)_defaultValue : (short)0;
				value._object = typeof(short);
				break;
			case FieldType.UShort:
				value._ushort = _defaultValue != null ? (ushort)_defaultValue : (ushort)0;
				value._object = typeof(ushort);
				break;
			case FieldType.Int:
				value._int = _defaultValue != null ? (int)_defaultValue : (int)0;
				value._object = typeof(int);
				break;
			case FieldType.UInt:
			case FieldType.Link:
				value._uint = _defaultValue != null ? (uint)_defaultValue : (uint)0;
				value._object = typeof(uint);
				break;
			case FieldType.Long:
				value._long = _defaultValue != null ? (long)_defaultValue : (long)0;
				value._object = typeof(long);
				break;
			case FieldType.ULong:
				value._ulong = _defaultValue != null ? (ulong)_defaultValue : (ulong)0;
				value._object = typeof(ulong);
				break;
			case FieldType.Float:
				value._float = _defaultValue != null ? (float)_defaultValue : (float)0;
				value._object = typeof(float);
			case FieldType.Double:
				value._double = _defaultValue != null ? (double)_defaultValue : (double)0;
				value._object = typeof(double);
			case FieldType.String:
				value._object = _defaultValue != null ? (string)_defaultValue : null;
				value._object = typeof(string);
			case FieldType.Block:
				value._ptr = null;
				value._object = null;
			case FieldType.Array:
				throw new InvalidOperationException ("Array is not a value.");
			}
		}

		public unsafe void GetValue(ref Value value, IntPtr ptr)
		{
			void* p = null;

			// Lookup a sparse value using sparse key/value array
			if (_isSparse) {
				SparseElement* sparseElem = (SparseElement*)ptr.ToPointer ();
				if (sparseElem != null) {
					while (sparseElem->CrcId != 0) {
						if (sparseElem->CrcId == _crcId) {
							p = (void*)(((byte*)sparseElem) + 4);
							break;
						}
						sparseElem++;
					}
				}
				// Sparse value doesn't exist.. return default value
				if (p == null) {
					GetDefaultValue (ref value);
					return;
				}
			} else {
				p = (byte*)ptr.ToPointer () + _start;
			}

			// Get value
			switch (_fieldType) {
			case FieldType.Bool:
				value._bool = *(bool*)p;
				value._object = typeof(bool);
				break;
			case FieldType.Byte:
				value._byte = *(byte*)p;
				value._object = typeof(byte);
				break;
			case FieldType.SByte:
				value._sbyte = *(sbyte*)p;
				value._object = typeof(sbyte);
				break;
			case FieldType.Char:
				value._char = *(char*)p;
				value._object = typeof(char);
				break;
			case FieldType.Short:
				value._short = *(short*)p;
				value._object = typeof(short);
				break;
			case FieldType.UShort:
				value._ushort = *(ushort*)p;
				value._object = typeof(ushort);
				break;
			case FieldType.Int:
				value._int = *(int*)p;
				value._object = typeof(int);
				break;
			case FieldType.UInt:
			case FieldType.Link:
				value._uint = *(uint*)p;
				value._object = typeof(uint);
				break;
			case FieldType.Long:
				value._long = *(long*)p;
				value._object = typeof(long);
				break;
			case FieldType.ULong:
				value._ulong = *(ulong*)p;
				value._object = typeof(ulong);
				break;
			case FieldType.Float:
				value._float = *(float*)p;
				value._object = typeof(float);
			case FieldType.Double:
				value._double = *(double*)p;
				value._object = typeof(double);
			case FieldType.String:
				value._object = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(new IntPtr(*(void**)p));
				value._object = typeof(string);
			case FieldType.Block:
				if (_blockDef == null) {
					// Handle ANY ref
					BlockElement* blockElem = (BlockElement*)((byte*)p + _start);
					value._ptr = new IntPtr (blockElem->Ptr);
					if (value._ptr != (void*)0)
						value._object = _schema.GetBlockByCrcId (blockElem->CrcId);
					else
						value._object = null;
				} else {
					// Handle specific ref
					value._ptr = new IntPtr((byte*)p + (index * _blockDef.Length));
					if (value._ptr != null)
						value._object = _blockDef;
					else
						value._object = null;
				}
			case FieldType.Array:
				throw new InvalidOperationException ("Array is not a value.");
			}
		}

		public unsafe void SetValue(ref Value value, IntPtr ptr) {
			void* p = null;

			// Lookup a sparse value using sparse key/value array
			if (_isSparse) {
				SparseElement* sparseElem = (SparseElement*)ptr.ToPointer ();
				if (sparseElem != null) {
					while (sparseElem->CrcId != 0) {
						if (sparseElem->CrcId == _crcId) {
							p = (void*)(((byte*)sparseElem) + 4);
							break;
						}
						sparseElem++;
					}
				}
				// Sparse value doesn't exist.. return default value
				if (p == null) {
					GetDefaultValue (ref value);
					return;
				}
			} else {
				p = (byte*)ptr.ToPointer () + _start;
			}

			switch (_fieldType) {
				case FieldType.Bool:
					*(bool*)p = value._bool;
					break;
				case FieldType.Byte:
					*(byte*)p = value._byte;
					break;
				case FieldType.SByte:
					*(sbyte*)p = value._sbyte;
					break;
				case FieldType.Char:
					*(char*)p = value._char;
					break;
				case FieldType.Short:
					*(short*)p = value._short;
					break;
				case FieldType.UShort:
					*(ushort*)p = value._ushort;
					break;
				case FieldType.Int:
					*(int*)p = value._int;
					break;
				case FieldType.UInt:
				case FieldType.Link:
					*(uint*)p = value._uint;
					break;
				case FieldType.Long:
					*(long*)p = value._long;
					break;
				case FieldType.ULong:
					*(ulong*)p = value._ulong;
					break;
				case FieldType.Float:
					*(float*)p= value._float;
					break;
				case FieldType.Double:
					*(double*)p = value._double;
					break;
				case FieldType.String:
					void* strptr = *(void**)p;
					if (strptr != null)
						AssetPool.FreeBuffer (strptr);
					*(void**)((byte*)p + _start) = AssetPool.AllocString(value._object as string).ToPointer();
					break;
				case FieldType.Block:
					if (_blockDef == null) {
						// Handle ANY ref
						BlockElement* blockElem = (BlockElement*)p + _start;
						value._object = _schema.GetBlockByCrcId (blockElem->CrcId);
						value._ptr = new IntPtr (blockElem->Ptr);
					} else {
						// Handle specific ref
						value._object = _blockDef;
						value._ptr = new IntPtr((byte*)p + (index * _blockDef.Length));
					}
					break;
				case FieldType.Array:
					throw new Exception ("Array values may not be set.");
			}
		}

		public unsafe void GetArrayValue(ref Value value, IntPtr ptr, int index) {
			if (_fieldType != FieldType.Array)
				throw new InvalidOperationException ("Not an array field.");
			void* p = (void*)((byte*)ptr.ToPointer() + _start);
			int len = *(int*)p;
			if (index < 0 || index >= len)
				throw new IndexOutOfRangeException ();
			p = *(void**)((byte*)p + 4);
			switch (_elementType) {
				case FieldType.Bool:
					value._bool = *(bool*)((bool*)p + index);
					break;
				case FieldType.Byte:
					value._byte = *(byte*)((byte*)p + index);
					break;
				case FieldType.SByte:
					value._sbyte = *(sbyte*)((sbyte*)p + index);
					break;
				case FieldType.Char:
					value._char = *(char*)((char*)p + index);
					break;
				case FieldType.Short:
					value._short = *(short*)((short*)p + index);
					break;
				case FieldType.UShort:
					value._ushort = *(ushort*)((ushort*)p + index);
					break;
				case FieldType.Int:
					value._int = *(int*)((int*)p + index);
					break;
				case FieldType.UInt:
				case FieldType.Link:
					value._uint = *(uint*)((uint*)p + index);
					break;
				case FieldType.Long:
					value._long = *(long*)((long*)p + index);
					break;
				case FieldType.ULong:
					value._ulong = *(ulong*)((ulong*)p + index);
					break;
				case FieldType.Float:
					value._float = *(float*)((float*)p + index);
					break;
				case FieldType.Double:
					value._double = *(double*)((double*)p + index);
					break;
				case FieldType.String:
					value._object = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(new IntPtr((byte**)p + index));
					break;
				case FieldType.Block:
					if (_blockDef == null) {
						// Handle ANY array
						BlockElement* blockElem = (BlockElement*)p + index;
						value._object = _schema.GetBlockByCrcId (blockElem->CrcId);
						value._ptr = new IntPtr (blockElem->Ptr);
					} else {
						// Handle homogonous array
						value._object = _blockDef;
						value._ptr = new IntPtr((byte*)p + (index * _blockDef.Length));
					}
					break;
				case FieldType.Array:
					throw new Exception ("Arrays in arrays are now allowed.");
			}
		}

		public unsafe void SetArrayValue(ref Value value, IntPtr ptr, int index) {
			if (_fieldType != FieldType.Array)
				throw new InvalidOperationException ("Not an array field.");
			void* p = (void*)((byte*)ptr.ToPointer() + _start);
			int len = *(int*)p;
			if (index < 0 || index >= len)
				throw new IndexOutOfRangeException ();
			p = *(void**)((byte*)p + 4);
			switch (_elementType) {
				case FieldType.Bool:
				*(bool*)((bool*)p + index) = value._bool;
				case FieldType.Byte:
				*(byte*)((byte*)p + index) = value._byte;
				case FieldType.SByte:
				*(sbyte*)((sbyte*)p + index) = value._sbyte;
				case FieldType.Char:
				*(char*)((char*)p + index) = value._char;
				case FieldType.Short:
				*(short*)((short*)p + index) = value._short;
				case FieldType.UShort:
				*(ushort*)((ushort*)p + index) = value._ushort;
				case FieldType.Int:
				*(int*)((int*)p + index) = value._int;
				case FieldType.UInt:
				case FieldType.Link:
				*(uint*)((uint*)p + index) = value._uint;
				case FieldType.Long:
				*(long*)((long*)p + index) = value._long;
				case FieldType.ULong:
				*(ulong*)((ulong*)p + index) = value._ulong;
				case FieldType.Float:
				*(float*)((float*)p + index) = value._float;
				case FieldType.Double:
				*(double*)((double*)p + index) = value._double;
				case FieldType.String:
				value._object = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(new IntPtr((byte**)p + index));
				case FieldType.Block:
				value._ptr = new IntPtr(*(void**)((byte**)p + index));
				case FieldType.Array:
				value._ptr = new IntPtr((void**)((byte**)p + index));
			}
		}

	}
}

