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
	public static class PSConverter
	{
		// deprecated, but we'll keep these around for awhile...
		public static bool 		ConvertToBool(object o) { return ToBool(o); }
		public static int 		ConvertToInt(object o) { return ToInt(o); }
		public static uint 		ConvertToUInt(object o) { return ToUInt(o); }
		public static float 	ConvertToFloat(object o) { return ToFloat(o); }
		public static double 	ConvertToDouble(object o) { return ToDouble(o); }
		public static string 	ConvertToString(object o) { return ToString(o); }

		//
		// casting conversions from string to value type
		//

		private static bool IsHexString(string s)
		{
			return (s.Length > 2) && (s[0] == '0') && (s[1] == 'x' || s[1] == 'X');
		}

		// Boolean(string)
		public static bool ToBool (string s)
		{
			Stats.Increment(StatsCounter.ConvertToInvoked);

			// note: we were wrong before, only the empty or null string produces false
			return !string.IsNullOrEmpty(s);
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

		// Number(string)
		public static double ToDouble (string s)
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

		//
		// casting conversions
		// 

		public static bool ToBool(object o)
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
				return ToBool((string)o);
			case TypeCode.Empty:
				return false;
			case TypeCode.Object:
				return (o != null);
			}
			return false;
		}

		public static uint ToUInt(double d)
		{
			return (uint)d;
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

		public static int ToInt(double d)
		{
			return (int)d;
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

		public static double ToDouble(int i)
		{
			return (double)i;
		}

		public static double ToDouble(uint u)
		{
			return (double)u;
		}

		public static double ToDouble(object o)
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
				return ToDouble((string)o);
			default:
				return double.NaN;
			}
		}

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
			return o.ToString();
		}

		// this specifically converts a * to an Object
		public static object UntypedToObject (object o)
		{
			Stats.Increment(StatsCounter.ConvertUntypedInvoked);

			if (o == PlayScript.Undefined._undefined) {
				return null; // only type "*" can be undefined
			}

			return o;
		}


		//
		// "as" operator conversion functions, these have different semantics from a normal cast
		//

		//
		// as bool
		//

		public static bool AsBool(bool b)
		{
			return b;
		}

		public static bool AsBool(int i)
		{
			return false;
		}

		public static bool AsBool(uint u)
		{
			return false;
		}

		public static bool AsBool(double d)
		{
			return false;
		}

		public static bool AsBool(string s)
		{
			return false;
		}

		public static bool AsBool(object o)
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

		public static bool AsUInt(bool b)
		{
			return false;
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

		public static double AsDouble(bool b)
		{
			return 0.0;
		}

		public static double AsDouble(int i)
		{
			return (double)i;
		}

		public static double AsDouble(uint u)
		{
			return (double)u;
		}

		public static double AsDouble(double d)
		{
			return d;
		}

		public static double AsDouble(string s)
		{
			return 0.0;
		}

		public static double AsDouble(object o)
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

	}

}
