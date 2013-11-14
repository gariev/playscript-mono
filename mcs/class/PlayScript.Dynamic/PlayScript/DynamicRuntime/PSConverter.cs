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
using System.Reflection;
using System.Collections;

namespace PlayScript.DynamicRuntime
{
	public static class PSConverter
	{
		public static object ConvertToString(object o, Type targetType)
		{
			return ConvertToString (o);
		}

		public static Func<object, Type, object> GetConversionFunction(object value, Type targetType, bool valueTypeIsConstant)
		{
			if (!valueTypeIsConstant) {
				// must use the slower convert method
				return Dynamic.ConvertValue;
			}

			if (value == null) {
				// no conversion required
				return null;
			}

			Type valueType = value.GetType();
			if (targetType == valueType) {
				// no conversion required
				return null;
			}

			if (targetType == typeof(System.Object)) {
				// no conversion required
				return null;
			}

			if (targetType.IsAssignableFrom(valueType)) {
				// no conversion required
				return null;
			} else {
				if (targetType == typeof(String)) {
					// conversion required
					return ConvertToString;
				}
				// conversion required
				return System.Convert.ChangeType;
			}
		}

		public static int ConvertToInt (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			if (o is int) {
				return (int)o;
			}
			if (PlayScript.Dynamic.IsNullOrUndefined (o)) {
				return 0;
			}

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
				return ConvertStringToInt((string)o);
			default:
				throw new Exception ("Invalid cast to int");
			}
		}

		public static uint ConvertToUInt (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			if (o is uint) {
				return (uint)o;
			}
			if (o is int) {
				return (uint)(int)o;
			}
			if (PlayScript.Dynamic.IsNullOrUndefined (o)) {
				return 0u;
			}

			TypeCode tc = Type.GetTypeCode(o.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)o ? 1u : 0u;
			case TypeCode.SByte:
				return (uint)(sbyte)o;
			case TypeCode.Byte:
				return (uint)(byte)o;
			case TypeCode.Int16:
				return (uint)(short)o;
			case TypeCode.UInt16:
				return (uint)(ushort)o;
			case TypeCode.Int32:
				return (uint)(int)o;
			case TypeCode.UInt32:
				return (uint)(uint)o;
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
				return ConvertStringToUInt((string)o);
			default:
				throw new Exception ("Invalid cast to uint");
			}
		}

		public static float ConvertToFloat (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			if (o is float) {
				return (float)o;
			} 
			if (o is double) {
				return (float)(double)o;
			}
			if (PlayScript.Dynamic.IsUndefined (o)) {
				return float.NaN;
			}
			if (o == null) {
				return 0.0f;
			}

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
				return ConvertStringToFloat((String)o);
			default:
				throw new Exception ("Invalid cast to float");
			}
		}

		public static double ConvertToDouble (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			if (o is double) {
				return (double)o;
			} 
			if (PlayScript.Dynamic.IsUndefined (o)) {
				return double.NaN;
			}
			if (o == null) {
				return 0.0;
			}

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
				return ConvertStringToDouble((String)o);
			default:
				throw new Exception ("Invalid cast to double");
			}
		}

		public static bool ConvertToBool (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			// handle most common cases first
			if (o == null) {
				return false;
			}

			if (o is bool) {
				return (bool)o;
			}

			if (o == PlayScript.Undefined._undefined) {
				return false;
			}

			var s = o as string;
			if (s != null) {
				return ConvertStringToBool(s);
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
			case TypeCode.Empty:
				return false;
			case TypeCode.Object:
				return (o != null);
			default:
				throw new Exception ("Invalid cast to bool");
			}
		}

		public static string ConvertToString (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			if (PlayScript.Dynamic.IsNullOrUndefined (o)) {
				return null;
			} else if  (o is string) {
				return (string)o;
			} else {
				return o.ToString ();
			}
		}

		public static object ConvertToObj (object o)
		{
			Stats.Increment(StatsCounter.ConvertBinderInvoked);

			if (o == PlayScript.Undefined._undefined) {
				return null; // only type "*" can be undefined
			}

			return o;
		}


		//
		// these are string -> value conversion functions
		//

		private static bool IsHexString(string s)
		{
			return (s.Length > 2) && (s[0] == '0') && (s[1] == 'x' || s[1] == 'X');
		}

		// Boolean(string)
		// note: we were wrong before, only the empty or null string produces false
		public static bool ConvertStringToBool (string s)
		{
			return !string.IsNullOrEmpty(s);
		}

		// int(string)
		public static int ConvertStringToInt (string s)
		{
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
		public static uint ConvertStringToUInt (string s)
		{
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
		public static float ConvertStringToFloat (string s)
		{
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
		public static double ConvertStringToDouble (string s)
		{
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


		// string as Boolean
		// note: we were wrong before, only the empty or null string produces false
		public static bool ConvertStringAsBool (string s)
		{
			return !string.IsNullOrEmpty(s);
		}

		// string as int
		// no parsing, returns 0
		public static int ConvertStringAsInt (string s)
		{
			return 0;
		}

		// string as uint
		// no parsing, returns 0
		public static uint ConvertStringAsUInt (string s)
		{
			return 0u;
		}

		// string as float
		// no parsing, returns 0
		public static float ConvertStringAsFloat (string s)
		{
			return 0.0f; 
		}

		// string as float
		// no parsing, returns 0
		public static double ConvertStringAsDouble (string s)
		{
			return 0.0; 
		}

	}

}
