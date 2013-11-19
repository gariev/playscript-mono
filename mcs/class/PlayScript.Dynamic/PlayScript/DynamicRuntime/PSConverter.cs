//
// PSConverter.cs
//
// Copyright 2013 Zynga Inc.
//	
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//		
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.

using System;
using System.Collections;

namespace PlayScript.DynamicRuntime
{
	//
	// This class performs conversions and casting of value types at runtime.
	// There are two distinct types of casts that need to be handled.
	//  1) The explicit casts:  i = int(obj); n = Number(obj); b = Boolean(obj); t = IInterface(obj)
	//  2) The "as" casts:  i = obj as int; n = obj as Number; t = obj as IInterface;
	//
	// The explicit casts are more forceful, they will do string parsing, will return NaNs on failure, 
	//  will not range check values, and will throw if class/interface not assignable. The explicit cast of null or undefined to a string
	//  will result in the string "null" or "undefined".
	// The "as" casts are more gentle, they will not do string parsing, will never return NaN,
	//  and will only return a value if it is fully representable in the target type. 
	//  If a cast fails, the result is zero or null. 
	//  For example:  (5 as uint) == 5;  (-5 as uint) == 0;  (2.0 as int) == 2; (2.5 as int) == 0.0
	//
	// To support both casting rules we expose ToInt and AsInt as seperate functions. They can be overloaded with the source value type
	// to reduce the amount of type checking. One version must accept an "object" to handle dynamic casts.
	//

	public static class PSConverter
	{
		// deprecated, but we'll keep these around for awhile...
		public static string 	ConvertToString(object o) { return ToString(o); }

		//
		// this is the implicit cast to string method
		// this is called when implicity casting from any reference type to string (but NOT when using function style casts)
		// note that this converts null and undefined to null
		//
		public static string ImplicitToString (object o)
		{
			if (o is string) {
				return (string)o;
			}
			if (o == null) {
				return null;
			}
			if (o == PlayScript.Undefined._undefined) {
				return null;
			}
			if (o is bool) {
				return PSConverter.ToString((bool)o);
			}
			return o.ToString();
		}

		// this is called when the toString method is called on a dynamic object
		public static string InvokeToString (object o)
		{
			return ToString(o);
		}

		// returns true if the string is possible hex (starts with 0x or 0X)
		public static bool IsHexString(string s)
		{
			return (s.Length > 2) && (s[0] == '0') && (s[1] == 'x' || s[1] == 'X');
		}

		//
		// casting conversions
		// 

		public static bool ToBoolean(bool b)
		{
			return b;
		}

		public static bool ToBoolean(int i)
		{
			return i != 0;
		}

		public static bool ToBoolean(uint u)
		{
			return u != 0;
		}

		public static bool ToBoolean(double d)
		{
			return d != 0.0;
		}

		// Boolean(string)
		public static bool ToBoolean (string s)
		{
			// note: we were wrong before, only the empty or null string produces false
			return !string.IsNullOrEmpty(s);
		}

		public static bool ToBoolean(object o)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (o == null) {
				return false;
			}

			if (o is bool) {
				return (bool)o;
			}

			if (o == PlayScript.Undefined._undefined) {
				return false;
			}

			TypeCode tc = Type.GetTypeCode(o.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)o;
			case TypeCode.SByte:
				return (sbyte)o != 0;
			case TypeCode.Byte:
				return (byte)o != 0;
			case TypeCode.Int16:
				return (short)o != 0;
			case TypeCode.UInt16:
				return (ushort)o != 0;
			case TypeCode.Int32:
				return (int)o != 0;
			case TypeCode.UInt32:
				return (uint)o != 0;
			case TypeCode.Int64:
				return (long)o != 0;
			case TypeCode.UInt64:
				return (ulong)o != 0;
			case TypeCode.Single:
				return (float)o != 0.0f;
			case TypeCode.Double:
				return (double)o != 0.0;
			case TypeCode.Decimal:
				return (decimal)o != 0;
			case TypeCode.String:
				return ToBoolean((string)o);
			case TypeCode.Empty:
				return false;
			case TypeCode.Object:
				return (o != null);
			}
			return false;
		}

		public static bool ImplicitToBoolean(bool b)
		{
			return b;
		}

		public static bool ImplicitToBoolean(int i)
		{
			return i != 0;
		}

		public static bool ImplicitToBoolean(uint u)
		{
			return u != 0;
		}

		public static bool ImplicitToBoolean(double d)
		{
			return d != 0.0;
		}

		public static bool ImplicitToBoolean(string s)
		{
			return ToBoolean(s);
		}

		public static bool ImplicitToBoolean(object o)
		{
			return ToBoolean(o);
		}

		public static int ToInt(bool b)
		{
			return b ? 1 : 0;
		}

		public static int ToInt(int i)
		{
			return i;
		}

		public static int ToInt(uint u)
		{
			return (int)u;
		}

		public static int ToInt(double d)
		{
			return (int)d;
		}

		// int(string)
		public static int ToInt (string s)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (IsHexString(s)) {
				return (int)Convert.ToUInt32(s, 16);
			} else {
				int value;
				if (int.TryParse(s, out value)) {
					return value;
				} else {
					return 0;
				}
			}
		}

		public static int ToInt(object o)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (o == null) return 0;

			TypeCode tc = Type.GetTypeCode(o.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)o ? 1 : 0;
			case TypeCode.SByte:
				return (sbyte)o;
			case TypeCode.Byte:
				return (byte)o;
			case TypeCode.Int16:
				return (short)o;
			case TypeCode.UInt16:
				return (ushort)o;
			case TypeCode.Int32:
				return (int)o;
			case TypeCode.UInt32:
				return (int)(uint)o;
			case TypeCode.Int64:
				return (int)(long)o;
			case TypeCode.UInt64:
				return (int)(ulong)o;
			case TypeCode.Single:
				return (int)(float)o;
			case TypeCode.Double:
				return (int)(double)o;
			case TypeCode.Decimal:
				return (int)(decimal)o;
			case TypeCode.String:
				return ToInt((string)o);
			default:
				return 0;
			}
		}

		public static uint ToUInt(bool b)
		{
			return b ? 1u : 0u;
		}

		public static uint ToUInt(int i)
		{
			return (uint)i;
		}

		public static uint ToUInt(uint u)
		{
			return u;
		}

		public static uint ToUInt(double d)
		{
			return (uint)d;
		}

		// uint(string)
		public static uint ToUInt (string s)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (IsHexString(s)) {
				return (uint)Convert.ToUInt32(s, 16);
			} else {
				uint value;
				if (uint.TryParse(s, out value)) {
					return value;
				} else {
					return 0u;
				}
			}
		}

		public static uint ToUInt(object o)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (o == null) return 0u;

			TypeCode tc = Type.GetTypeCode(o.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)o ? 1u : 0u;
			case TypeCode.SByte:
				return (uint)(sbyte)o;
			case TypeCode.Byte:
				return (byte)o;
			case TypeCode.Int16:
				return (uint)(short)o;
			case TypeCode.UInt16:
				return (ushort)o;
			case TypeCode.Int32:
				return (uint)(int)o;
			case TypeCode.UInt32:
				return (uint)o;
			case TypeCode.Int64:
				return (uint)(long)o;
			case TypeCode.UInt64:
				return (uint)(ulong)o;
			case TypeCode.Single:
				return (uint)(float)o;
			case TypeCode.Double:
				return (uint)(double)o;
			case TypeCode.Decimal:
				return (uint)(decimal)o;
			case TypeCode.String:
				return ToUInt((string)o);
			default:
				return 0u;
			}
		}


		
		// float(string)
		public static float ToFloat (string s)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (IsHexString(s)) {
				return (float)Convert.ToUInt32(s, 16);
			} else {
				float value;
				if (float.TryParse(s, out value)) {
					return value;
				} else {
					return float.NaN; // return NaN on failure
				}
			}
		}

		public static float ToFloat(object o)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (o == null) return 0.0f;
			if (o == PlayScript.Undefined._undefined) return float.NaN;

			TypeCode tc = Type.GetTypeCode(o.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)o ? 1.0f : 0.0f;
			case TypeCode.SByte:
				return (sbyte)o;
			case TypeCode.Byte:
				return (byte)o;
			case TypeCode.Int16:
				return (short)o;
			case TypeCode.UInt16:
				return (ushort)o;
			case TypeCode.Int32:
				return (int)o;
			case TypeCode.UInt32:
				return (uint)o;
			case TypeCode.Int64:
				return (long)o;
			case TypeCode.UInt64:
				return (ulong)o;
			case TypeCode.Single:
				return (float)o;
			case TypeCode.Double:
				return (float)(double)o;
			case TypeCode.Decimal:
				return (float)(decimal)o;
			case TypeCode.String:
				return ToFloat((string)o);
			default:
				return float.NaN;
			}
		}

		public static double ToNumber(bool b)
		{
			return b ? 1.0 : 0.0;
		}

		public static double ToNumber(int i)
		{
			return (double)i;
		}

		public static double ToNumber(uint u)
		{
			return (double)u;
		}

		public static double ToNumber(double d)
		{
			return d;
		}

		public static double ToNumber(float f)
		{
			return (double)f;
		}
		
		// Number(string)
		public static double ToNumber (string s)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (IsHexString(s)) {
				return (double)Convert.ToUInt32(s, 16);
			} else {
				double value;
				if (double.TryParse(s, out value)) {
					return value;
				} else {
					return double.NaN; // return NaN on failure
				}
			}
		}

		public static double ToNumber(object o)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (o == null) return 0.0;
			if (o == PlayScript.Undefined._undefined) return double.NaN;

			TypeCode tc = Type.GetTypeCode(o.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)o ? 1.0 : 0.0;
			case TypeCode.SByte:
				return (sbyte)o;
			case TypeCode.Byte:
				return (byte)o;
			case TypeCode.Int16:
				return (short)o;
			case TypeCode.UInt16:
				return (ushort)o;
			case TypeCode.Int32:
				return (int)o;
			case TypeCode.UInt32:
				return (uint)o;
			case TypeCode.Int64:
				return (long)o;
			case TypeCode.UInt64:
				return (ulong)o;
			case TypeCode.Single:
				return (float)o;
			case TypeCode.Double:
				return (double)o;
			case TypeCode.Decimal:
				return (double)(decimal)o;
			case TypeCode.String:
				return ToNumber((string)o);
			default:
				return double.NaN;
			}
		}

		public static string ToString(bool b)
		{
			return b ? "true" : "false";
		}

		public static string ToString(int i)
		{
			return i.ToString();
		}

		public static string ToString(uint u)
		{
			return u.ToString();
		}

		public static string ToString(double d)
		{
			return d.ToString();
		}

		// this is the explicit string conversion method:
		//   var str:String = String(o);
		// note that this converts null and undefined to their string counterparts
		public static string ToString(object o)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			if (o is string) {
				return (string)o;
			}
			if (o == null) {
				return "null";
			}
			if (o == PlayScript.Undefined._undefined) {
				return "undefined";
			}
			if (o is bool) {
				return ToString((bool)o);
			}
			return o.ToString();
		}

		// this explicitly converts a * to an Object
		public static dynamic UntypedToObject (object o)
		{
			Stats.Increment(StatsCounter.ConvertUntypedInvoked);

			if (o == PlayScript.Undefined._undefined) {
				return null; // only type "*" can be undefined
			}

			return o;
		}

		// this implicitly converts a * to an Object
		public static dynamic UntypedImplicitToObject (object o)
		{
			Stats.Increment(StatsCounter.ConvertUntypedInvoked);

			if (o == PlayScript.Undefined._undefined) {
				return null; // only type "*" can be undefined
			}

			return o;
		}

		// this converts a * to an Object with "as"
		public static dynamic UntypedAsObject (object o)
		{
			Stats.Increment(StatsCounter.ConvertUntypedInvoked);

			if (o == PlayScript.Undefined._undefined) {
				return null; // only type "*" can be undefined
			}

			return o;
		}

		[return: AsUntyped]
		public static dynamic ObjectImplicitToUntyped (object o)
		{
			return o;
		}

		public static dynamic ToObject (object o)
		{
			return o;
		}

		[return: AsUntyped]
		public static dynamic ToUntyped (object o)
		{
			return o;
		}

		// this converts a * to a Variant
		public static Variant ToVariant(object o)
		{
			return Variant.FromAnyType(o);
		}

		//
		// "as" operator conversion functions, these have different semantics from a normal cast
		//

		//
		// as bool
		//

		public static bool AsBoolean(bool b)
		{
			return b;
		}

		public static bool AsBoolean(int i)
		{
			return false;
		}

		public static bool AsBoolean(uint u)
		{
			return false;
		}

		public static bool AsBoolean(double d)
		{
			return false;
		}

		public static bool AsBoolean(string s)
		{
			return false;
		}

		public static bool AsBoolean(object o)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);

			if (o is bool) {
				return (bool)o;
			}
			return false;
		}

		//
		// as int
		//

		public static int AsInt(bool b)
		{
			return 0;
		}

		public static int AsInt(int i)
		{
			return i;
		}

		public static int AsInt(uint u)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);
			if (u > int.MaxValue) {
				// value could not be represented
				return 0;
			}

			// cast to integer
			return (int)u;
		}

		public static int AsInt(double d)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);
			// cast to target type
			int value = (int)d;
			// if the value is preserved then it can successfully be cast using as
			if ( ((double)value) == d ){
				return value; 
			} else {
				// value could not be represented
				return 0;
			}
		}

		public static int AsInt(string s)
		{
			return 0;
		}

		public static int AsInt(object o)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);
			if (o is int) {
				return (int)o;
			}
			if (o is uint) {
				return AsInt((uint)o);
			}
			if (o is double) {
				return AsInt((double)o);
			}
			return 0;
		}

		//
		// as uint
		//

		public static uint AsUInt(bool b)
		{
			return 0u;
		}

		public static uint AsUInt(int i)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);

			if (i < uint.MinValue) {
				// value could not be represented, return zero
				return 0u;
			}

			// cast to uint
			return (uint)i;
		}

		public static uint AsUInt(uint u)
		{
			return u;
		}

		public static uint AsUInt(double d)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);

			// cast to target type
			uint value = (uint)d;
			// if the value is preserved then it can successfully be cast using as
			if ( ((double)value) == d ){
				return value; 
			} else {
				// value could not be represented
				return 0u;
			}
		}

		public static uint AsUInt(string s)
		{
			return 0;
		}

		public static uint AsUInt(object o)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);

			if (o is uint) {
				return (uint)o;
			}
			if (o is int) {
				return AsUInt((int)o);
			}
			if (o is double) {
				return AsUInt((double)o);
			}
			return 0u;
		}


		//
		// as double 
		//

		public static double AsNumber(bool b)
		{
			return 0.0;
		}

		public static double AsNumber(int i)
		{
			return (double)i;
		}

		public static double AsNumber(uint u)
		{
			return (double)u;
		}

		public static double AsNumber(double d)
		{
			return d;
		}

		public static double AsNumber(string s)
		{
			return 0.0;
		}

		public static double AsNumber(object o)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);

			if (o is double) {
				return (double)o;
			}
			if (o is int) {
				return (double)(int)o;
			}
			if (o is uint) {
				return (double)(uint)o;
			}
			if (o is float) {
				return (double)(float)o;
			}
			return 0.0;
		}

		//
		// as float
		//

		
		public static float AsFloat(bool b)
		{
			return 0.0f;
		}

		public static float AsFloat(int i)
		{
			return (float)i;
		}

		public static float AsFloat(uint u)
		{
			return (float)u;
		}

		public static float AsFloat(double d)
		{
			return (float)d;
		}

		public static float AsFloat(string s)
		{
			return 0.0f;
		}
		
		public static float AsFloat(object o)
		{
			Stats.Increment(StatsCounter.ConvertAsInvoked);

			if (o is float) {
				return (float)o;
			}
			if (o is double) {
				return (float)(double)o;
			}
			if (o is int) {
				return (float)(int)o;
			}
			if (o is uint) {
				return (float)(uint)o;
			}
			return 0.0f;
		}

		//
		// as string
		//

		public static string AsString(bool b)
		{
			return null;
		}

		public static string AsString(int i)
		{
			return null;
		}

		public static string AsString(uint u)
		{
			return null;
		}

		public static string AsString(double d)
		{
			return null;
		}

		public static string AsString(string s)
		{
			return s;
		}

		public static string AsString(object o)
		{
			return o as string;
		}


	}

}
