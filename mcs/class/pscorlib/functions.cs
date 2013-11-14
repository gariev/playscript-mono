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

namespace _root
{




	//
	// Conversions (must be in C# to avoid conflicts).
	//

	public static class String_fn
	{
		public static string CastToString (object o)
		{
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

		public static string String (object o)
		{
			return o.ToString();
		}

		public static string String (string s)
		{
			return s;
		}

		public static string String (int i)
		{
			return i.ToString ();
		}

		public static string String (uint u)
		{
			return u.ToString ();
		}

		public static string String (double d)
		{
			return d.ToString ();
		}

		public static string String (bool b)
		{
			return b.ToString ();
		}

	}

	public static class Number_fn
	{  
		// Inlineable method
		public static double Number (string s)
		{
			return PlayScript.DynamicRuntime.PSConverter.ConvertStringToDouble(s);
		}

		public static double Number (object o)
		{
			return PlayScript.DynamicRuntime.PSConverter.ConvertToDouble(o);
		}
	}

	public static class int_fn
	{  
		// Inlineable method
		public static int @int (string s)
		{
			return PlayScript.DynamicRuntime.PSConverter.ConvertStringToInt(s);
		}

		public static int @int (object o)
		{
			return PlayScript.DynamicRuntime.PSConverter.ConvertToInt(o);
		}
		
	}

	public static class uint_fn
	{  

		// Inlineable method
		public static uint @uint (string s)
		{
			return PlayScript.DynamicRuntime.PSConverter.ConvertStringToUInt(s);
		}

		public static uint @uint (object o)
		{
			return PlayScript.DynamicRuntime.PSConverter.ConvertToUInt(o);
		}

	}

	public static class Boolean_fn
	{  

		// Not inlinable.. but required to get correct results in flash.
		public static bool Boolean (object d)
		{
			// pass through to PSConverter
			return PlayScript.DynamicRuntime.PSConverter.ConvertToBool(d);
		}
	}

	public static class _typeof_fn
	{
		public static string _typeof (object d) {
			if (d == null || d == PlayScript.Undefined._undefined) 
				return "undefined";
			
			if (d is XML || d is XMLList)
				return "xml";
			
			if (d is Delegate)
				return "function";
			
			TypeCode tc = Type.GetTypeCode(d.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return "boolean";
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return "number";
			default:
				return "object";
			}
		}
	}

}

