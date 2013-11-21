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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Diagnostics;
using PlayScript.DynamicRuntime;

namespace PlayScript
{
	// this struct can hold any playscript value 
	// this is used to prevent unnecessary boxing of value types (bool/int/number etc)
	[DebuggerDisplay("{Type} {Value.IntValue} {Value.NumberValue} {Reference}")]
#if PLATFORM_MONOTOUCH || PLATFORM_MONODROID
	[StructLayout(LayoutKind.Sequential, Size=16)]
#endif
	public struct Variant : IEquatable<Variant>
	{
		// type code for variant
		public enum TypeCode
		{
			Undefined,
			Null,
			Boolean,
			Int,
			UInt,
			Number,
			String,
			Object
		};

		// union containing value data
		[StructLayout(LayoutKind.Explicit, Size=8)]
		public struct ValueData
		{
			[FieldOffset(0)]
			public double		NumberValue;
			[FieldOffset(0)]
			public int			IntValue;
			[FieldOffset(0)]
			public uint			UIntValue;
			[FieldOffset(0)]
			public bool			BoolValue;
		}

		// depending on alignment requirements and reference size we may have to layout the structure differently
		public readonly TypeCode 	Type;
		public object 				Reference;	
		public ValueData			Value;

		// property returning the string reference
		public string 				StringReference 	{get {return (string)Reference;}}

		//
		// constructors for different types
		// unfortunately all fields must be initialized even if they overlap in a union
		//

		// undefined value
		public static Variant Undefined = new Variant(TypeCode.Undefined);

		// null value
		public static Variant Null = new Variant(TypeCode.Null);

		public Variant(TypeCode type)
		{
			Type = type;
			Reference = null;
			Value = new ValueData();
		}

		public Variant(TypeCode type, object reference, ValueData data)
		{
			Type = type;
			Reference = reference;
			Value = data;
		}

		public Variant(bool value)
		{
			Type = TypeCode.Boolean;
			Reference = value ? sBoolTrue : sBoolFalse;
			Value = new ValueData();
			Value.BoolValue = value;
		}

		public Variant(int value, object boxedValue = null)
		{
			Type = TypeCode.Int;
			Reference = boxedValue;
			Value = new ValueData();
			Value.IntValue = value;
		}

		public Variant(uint value, object boxedValue = null)
		{
			Type = TypeCode.UInt;
			Reference = boxedValue;
			Value = new ValueData();
			Value.UIntValue = value;
		}

		public Variant(double value, object boxedValue = null)
		{
			Type = TypeCode.Number;
			Reference = boxedValue;
			Value = new ValueData();
			Value.NumberValue = value;
		}

		public Variant(string value)
		{
			Type = TypeCode.String;
			Reference = value;
			Value = new ValueData();
		}

		public Variant(object value)
		{
			Type = TypeCode.Object;
			Reference = value;
			Value = new ValueData();
		}


		public bool IsDefined
		{
			get
			{
				return Type != TypeCode.Undefined;
			}
		}

		public bool IsUndefined
		{
			get
			{
				return Type == TypeCode.Undefined;
			}
		}

		public bool IsNull
		{
			get
			{
				return Type == TypeCode.Null;
			}
		}

		public bool IsBoolean
		{
			get
			{
				return Type == TypeCode.Boolean;
			}
		}

		public bool IsInt
		{
			get
			{
				return Type == TypeCode.Int;
			}
		}

		public bool IsUInt
		{
			get
			{
				return Type == TypeCode.UInt;
			}
		}

		public bool IsNumber
		{
			get
			{
				return Type == TypeCode.Number;
			}
		}

		public bool IsString
		{
			get
			{
				return Type == TypeCode.String;
			}
		}

		public bool IsNumeric
		{
			get
			{
				return Type == TypeCode.Int || Type == TypeCode.UInt || Type == TypeCode.Number;
			}
		}

		public bool IsReference
		{
			get 
			{
				return Reference != null;
			}
		}

		public bool IsBoxed
		{
			get 
			{
				return (Type < TypeCode.String) && (Reference != null);
			}
		}

		// set to true if the value is the default (false,0,null)
		public bool HasDefaultValue
		{
			get 
			{
				switch (Type) {
				case TypeCode.Undefined:
					return true; 
				case TypeCode.Null:
					return true;
				case TypeCode.Boolean:
					return Value.BoolValue == false;
				case TypeCode.Int:
					return Value.IntValue == 0;
				case TypeCode.UInt:
					return Value.UIntValue == 0;
				case TypeCode.Number:
					return Value.NumberValue == 0.0;
				case TypeCode.String:
					return Reference == null;
				case TypeCode.Object:
					return Reference == null;
				default:
					throw new InvalidCastException();
				}
			}
		}

		//
		// conversion operators (variant -> system types)
		//

		public dynamic ToDynamic()
		{
			return (dynamic)ToObject();
		}

		// returns an object or a reference to undefined
		[return: AsUntyped]
		public object ToUntyped()
		{
			if (Type == TypeCode.Undefined) {
				return PlayScript.Undefined._undefined;
			} else {
				return ToObject();
			}
		}

		// returns a boxed object
		// will return null if undefined
		public object ToObject(object defaultValue = null)
		{
			// return referenced or boxed object if we have it
			if (Reference != null) {
				return Reference;
			}

			// box value types to number and cache boxed object in our reference
			switch (Type) {
			case TypeCode.Undefined:
				// return default value (do not cache it)
				return defaultValue;
			case TypeCode.Null:
				// return null (do not return default value)
				return null;
			case TypeCode.Boolean:
				Reference = Value.BoolValue ? sBoolTrue : sBoolFalse;
				break;
			case TypeCode.Int:
				if (Value.IntValue == 0) Reference = sIntZero; else 
				if (Value.IntValue == 1) Reference = sIntOne; else 
				if (Value.IntValue ==-1) Reference = sIntNegOne; else
					Reference = (object)Value.IntValue;	// box integer
				break;
			case TypeCode.UInt:
				if (Value.UIntValue == 0u) Reference = sUIntZero; else 
				if (Value.UIntValue == 1u) Reference = sUIntOne; else
					Reference = (object)Value.UIntValue;	// box integer
				break;
			case TypeCode.Number:
				if (Value.NumberValue == 0.0) Reference = sNumberZero; else
				if (Value.NumberValue == 1.0) Reference = sNumberOne; else 
					Reference = (object)Value.NumberValue; // box number
				break;
			}

			return Reference;
		}


		public int ToInt(int defaultValue)
		{
			// return default value only in the undefined case
			if (Type == TypeCode.Undefined) {
				return defaultValue;
			} else {
				return ToInt();
			}
		}

		public int ToInt()
		{
			if (Type == TypeCode.Int) {
				return Value.IntValue;
			}

			switch (Type) {
			case TypeCode.Undefined:
				return 0;
			case TypeCode.Null:
				return 0;
			case TypeCode.Boolean:
				return Value.BoolValue ? 1 : 0;
			case TypeCode.Int:
				return Value.IntValue;
			case TypeCode.UInt:
				return (int)Value.UIntValue;
			case TypeCode.Number:
				return (int)Value.NumberValue;
			case TypeCode.String:
				return PSConverter.ToInt(StringReference);
			case TypeCode.Object:
				return 0;
			default:
				throw new InvalidCastException("Cannot cast to Int");
			}
		}

		public int AsInt()
		{
			if (Type == TypeCode.Int) {
				return Value.IntValue;
			}
			if (Type == TypeCode.Number) {
				return PSConverter.AsInt(Value.NumberValue);
			}
			if (Type == TypeCode.UInt) {
				return PSConverter.AsInt(Value.UIntValue);
			}
			return 0;
		}

		public uint ToUInt(uint defaultValue)
		{
			// return default value only in the undefined case
			if (Type == TypeCode.Undefined) {
				return defaultValue;
			} else {
				return ToUInt();
			}
		}

		public uint ToUInt()
		{
			if (Type == TypeCode.UInt) {
				return Value.UIntValue;
			}
			switch (Type) {
			case TypeCode.Undefined:
				return 0u;
			case TypeCode.Null:
				return 0u;
			case TypeCode.Boolean:
				return Value.BoolValue ? 1u : 0u;
			case TypeCode.Int:
				return (uint)Value.IntValue;
			case TypeCode.UInt:
				return Value.UIntValue;
			case TypeCode.Number:
				return (uint)Value.NumberValue;
			case TypeCode.String:
				return PSConverter.ToUInt(StringReference);
			case TypeCode.Object:
				return 0u;
			default:
				throw new InvalidCastException("Cannot cast to UInt");
			}
		}

		public uint AsUInt()
		{
			if (Type == TypeCode.UInt) {
				return Value.UIntValue;
			}
			if (Type == TypeCode.Int) {
				return PSConverter.AsUInt(Value.IntValue);
			}
			if (Type == TypeCode.Number) {
				return PSConverter.AsUInt(Value.NumberValue);
			}
			return 0u;
		}

		public bool ToBoolean(bool defaultValue)
		{
			// return default value only in the undefined case
			if (Type == TypeCode.Undefined) {
				return defaultValue;
			} else {
				return ToBoolean();
			}
		}

		public bool ToBoolean()
		{
			if (Type == TypeCode.Boolean) {
				return Value.BoolValue;
			}
			switch (Type) {
			case TypeCode.Undefined:
				return false;
			case TypeCode.Null:
				return false;
			case TypeCode.Boolean:
				return Value.BoolValue;
			case TypeCode.Int:
				return Value.IntValue != 0;
			case TypeCode.UInt:
				return Value.UIntValue != 0u;
			case TypeCode.Number:
				return Value.NumberValue != 0.0;
			case TypeCode.String:
				return PSConverter.ToBoolean(StringReference);
			case TypeCode.Object:
				return Reference != null;
			default:
				throw new InvalidCastException("Cannot cast to UInt");
			}
		}

		public bool AsBoolean()
		{
			if (Type == TypeCode.Boolean) {
				return Value.BoolValue;
			} else {
				return false;
			}
		}

		public double ToNumber(double defaultValue)
		{
			// return default value only in the undefined case
			if (Type == TypeCode.Undefined) {
				return defaultValue;
			} else {
				return ToNumber();
			}
		}

		public double ToNumber()
		{
			if (Type == TypeCode.Number) {
				return (double)Value.NumberValue;
			}
			switch (Type) {
			case TypeCode.Undefined:
				return double.NaN;
			case TypeCode.Null:
				return 0.0;
			case TypeCode.Boolean:
				return Value.BoolValue ? 1.0 : 0.0;
			case TypeCode.Int:
				return (double)Value.IntValue;
			case TypeCode.UInt:
				return (double)Value.UIntValue;
			case TypeCode.Number:
				return Value.NumberValue;
			case TypeCode.String:
				return PSConverter.ToNumber(StringReference);
			case TypeCode.Object:
				return double.NaN;
			default:
				throw new InvalidCastException("Cannot cast to Number");
			}
		}

		public double AsNumber()
		{
			if (Type == TypeCode.Number) {
				return (double)Value.NumberValue;
			}
			if (Type == TypeCode.Int) {
				return (double)Value.IntValue;
			}
			if (Type == TypeCode.UInt) {
				return (double)Value.UIntValue;
			}
			return 0.0f;
		}

		public float ToFloat(float defaultValue)
		{
			// return default value only in the undefined case
			if (Type == TypeCode.Undefined) {
				return defaultValue;
			} else {
				return ToFloat();
			}
		}

		public float ToFloat()
		{
			if (Type == TypeCode.Number) {
				return (float)Value.NumberValue;
			}
			switch (Type) {
			case TypeCode.Undefined:
				return float.NaN;
			case TypeCode.Null:
				return 0.0f;
			case TypeCode.Boolean:
				return Value.BoolValue ? 1.0f : 0.0f;
			case TypeCode.Int:
				return (float)Value.IntValue;
			case TypeCode.UInt:
				return (float)Value.UIntValue;
			case TypeCode.Number:
				return (float)Value.NumberValue;
			case TypeCode.String:
				return (float)PSConverter.ToNumber(StringReference);
			case TypeCode.Object:
				return float.NaN;
			default:
				throw new InvalidCastException("Cannot cast to Float");
			}
		}

		public float AsFloat()
		{
			if (Type == TypeCode.Number) {
				return (float)Value.NumberValue;
			}
			if (Type == TypeCode.Int) {
				return (float)Value.IntValue;
			}
			if (Type == TypeCode.UInt) {
				return (float)Value.UIntValue;
			}
			return 0.0f;
		}

		public string ToString(string defaultValue)
		{
			// return default value only in the undefined case
			if (Type == TypeCode.Undefined) {
				return defaultValue;
			} else {
				return ToString();
			}
		}

		public override string ToString()
		{
			if (Type == TypeCode.String) {
				return StringReference;
			}
			switch (Type) {
			case TypeCode.Undefined:
				return "undefined";
			case TypeCode.Null:
				return "null";
			case TypeCode.Boolean:
				return Value.BoolValue ? "true" : "false";
			case TypeCode.Int:
				return Value.IntValue.ToString();
			case TypeCode.UInt:
				return Value.UIntValue.ToString();
			case TypeCode.Number:
				return Value.NumberValue.ToString();
			case TypeCode.String:
				return StringReference;
			case TypeCode.Object:
				return Reference.ToString();
			default:
				throw new InvalidCastException("Cannot cast to String");
			}
		}

		public string AsString()
		{
			return Reference as string;
		}

		public object ToType(System.Type type)
		{
			var typeCode = System.Type.GetTypeCode (type);
			switch (typeCode) {
			case System.TypeCode.Int32:
				return ToInt();
			case System.TypeCode.Double:
				return ToNumber();
			case System.TypeCode.Boolean:
				return ToBoolean();
			case System.TypeCode.UInt32:
				return ToUInt();
			case System.TypeCode.Single:
				return ToFloat();
			case System.TypeCode.String:
				return ToString();
			case System.TypeCode.Object:
				return ToObject();
			default:
				throw new InvalidCastException ("Invalid cast to type:" + type.ToString());
			}
		}

		// implicit conversions to variant
		public static implicit operator Variant(bool value)
		{
			return new Variant(value);
		}

		public static implicit operator Variant(int value)
		{
			return new Variant(value);
		}

		public static implicit operator Variant(uint value)
		{
			return new Variant(value);
		}

		public static implicit operator Variant(double value)
		{
			return new Variant(value);
		}

		public static implicit operator Variant(string value)
		{
			return new Variant(value);
		}

		// conversions from variant (should these be implicit or explicit?
		public static explicit operator bool(Variant variant)
		{
			return variant.ToBoolean();
		}
		public static explicit operator int(Variant variant)
		{
			return variant.ToInt();
		}
		public static explicit operator uint(Variant variant)
		{
			return variant.ToUInt();
		}
		public static explicit operator double(Variant variant)
		{
			return variant.ToNumber();
		}
		public static explicit operator string(Variant variant)
		{
			return variant.ToString();
		}

		// creates a variant from an object, examining the object's type appropriately
		public static Variant FromAnyType(object o)
		{
			if (o == null) {
				return new Variant(TypeCode.Null);
			}
			if (o == PlayScript.Undefined._undefined) {
				return new Variant(TypeCode.Undefined);
			}

			var typeCode = System.Type.GetTypeCode (o.GetType());
			switch (typeCode) {
			case System.TypeCode.Int32:
				return new Variant((int)o, o);
			case System.TypeCode.Single:
				return new Variant((double)(float)o);
			case System.TypeCode.Double:
				return new Variant((double)o, o);
			case System.TypeCode.Boolean:
				return new Variant((bool)o);
			case System.TypeCode.UInt32:
				return new Variant((uint)o, o);
			case System.TypeCode.String:
				return new Variant((string)o);
			case System.TypeCode.Object:
				return new Variant(o);
			default:
				throw new InvalidCastException ("Invalid cast to type:" + o.GetType().ToString());
			}
		}

		// creates a variant from a reference type of T
		// no checks for primitive types needed
		public static Variant FromReference<T>(T o) where T:class
		{
			return new Variant((object)o);
		}

		// casts to a reference type of T, throws if cast fails
		public T ToReference<T>() where T:class
		{
			return (T)Reference;
		}

		// casts to a reference type of T, returns null if cast fails
		public T AsReference<T>() where T:class
		{
			return Reference as T;
		}

		// casts true if variant is to a reference type of T
		public bool IsReference<T>() where T:class
		{
			return Reference is T;
		}


		#region IEquatable implementation
		public bool Equals(Variant other)
		{
			if (this.Type != other.Type) {
				// compare numeric values by promoting them
				if (this.IsNumeric && other.IsNumeric) {
					return this.ToNumber() == other.ToNumber();
				}

				// TODO we should do some type conversion here
				return false;
			}

			// they are both the same type
			switch (Type) {
			case TypeCode.Undefined:
				return false;
			case TypeCode.Null:
				return true;
			case TypeCode.Boolean:
				return this.Value.BoolValue == other.Value.BoolValue;
			case TypeCode.Int:
				return this.Value.IntValue == other.Value.IntValue;
			case TypeCode.UInt:
				return this.Value.UIntValue == other.Value.UIntValue;
			case TypeCode.Number:
				return this.Value.NumberValue == other.Value.NumberValue;
			case TypeCode.String:
				return this.StringReference == other.StringReference;
			case TypeCode.Object:
				return this.Reference.Equals(other.Reference);
			default:
				throw new InvalidCastException(Type.ToString());
			}

		}
		#endregion


		// pre-boxed values
		private static readonly object sBoolTrue = (object)true;
		private static readonly object sBoolFalse = (object)false;
		private static readonly object sIntNegOne = (object)(int)-1;
		private static readonly object sIntZero = (object)(int)0;
		private static readonly object sIntOne = (object)(int)1;
		private static readonly object sUIntZero = (object)(uint)0;
		private static readonly object sUIntOne = (object)(uint)1;
		private static readonly object sNumberZero = (object)(double)0.0;
		private static readonly object sNumberOne = (object)(double)1.0;

	};
}
