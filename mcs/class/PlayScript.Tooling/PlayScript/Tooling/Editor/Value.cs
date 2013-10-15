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
		[FieldOffset(0)]public short _short;
		[FieldOffset(0)]public ushort _ushort;
		[FieldOffset(0)]public int _int;
		[FieldOffset(0)]public uint _uint;
		[FieldOffset(0)]public long _long;
		[FieldOffset(0)]public ulong _ulong;
		[FieldOffset(0)]public float _float;
		[FieldOffset(0)]public double _double;
		[FieldOffset(8)]public object _object;   // Also indicates the type of the values above.
	}
}

