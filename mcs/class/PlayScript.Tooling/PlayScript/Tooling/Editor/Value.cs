using System;
using System.Runtime.InteropServices;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Simple struct to facilitate passing values to and from getter/setter methods.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size=16)]
	public struct Value
	{
		[FieldOffset(0)]public bool _bool;
		[FieldOffset(0)]public byte _byte;
		[FieldOffset(0)]public sbyte _sbyte;
		[FieldOffset(0)]public char _char;
		[FieldOffset(0)]public short _short;
		[FieldOffset(0)]public ushort _ushort;
		[FieldOffset(0)]public int _int;
		[FieldOffset(0)]public uint _uint;
		[FieldOffset(0)]public long _long;
		[FieldOffset(0)]public ulong _ulong;
		[FieldOffset(0)]public float _float;
		[FieldOffset(0)]public double _double;
		[FieldOffset(0)]public IntPtr _ptr;
		[FieldOffset(8)]public object _object;   // Also indicates the type of the values above.

		public Value(bool v) {
			_bool = v;
			_object = typeof(bool);
		}

		public Value(byte v) {
			_byte = v;
			_object = typeof(byte);
		}

		public Value(sbyte v) {
			_sbyte = v;
			_object = typeof(sbyte);
		}

		public Value(char v) {
			_char = v;
			_object = typeof(char);
		}

		public Value(short v) {
			_short = v;
			_object = typeof(short);
		}

		public Value(ushort v) {
			_ushort = v;
			_object = typeof(ushort);
		}

		public Value(int v) {
			_int = v;
			_object = typeof(int);
		}

		public Value(uint v) {
			_uint = v;
			_object = typeof(uint);
		}

		public Value(long v) {
			_long = v;
			_object = typeof(long);
		}

		public Value(ulong v) {
			_ulong = v;
			_object = typeof(ulong);
		}

		public Value(float v) {
			_float = v;
			_object = typeof(float);
		}

		public Value(double v) {
			_double = v;
			_object = typeof(double);
		}

		public Value(IntPtr v) {
			_ptr = v;
			_object = typeof(IntPtr);
		}

		public Value(object v) {
			_object = v;
		}

		public object ToValue() {
			var type = _object as Type;
			if (type == null) {
				switch (Type.GetTypeCode(type)) {
				case TypeCode.Boolean:
					return _bool;
				case TypeCode.Byte:
					return _byte;
				case TypeCode.SByte:
					return _sbyte;
				case TypeCode.Char:
					return _char;
				case TypeCode.Int16:
					return _short;
				case TypeCode.UInt16:
					return _ushort;
				case TypeCode.Int32:
					return _int;
				case TypeCode.UInt32:
					return _uint;
				case TypeCode.Int64:
					return _long;
				case TypeCode.UInt64:
					return _ulong;
				case TypeCode.Single:
					return _float;
				case TypeCode.Double:
					return _double;
				default:
					if (type == typeof(IntPtr))
						return _ptr;
					return _object;
				}
			}
		}
	}
}

