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
	// interface for an array of variants
	// it has a length, each element has a type and a value
	public interface IVariantArray
	{
		// returns length of array
		int 					Length {get;}
		// gets the type for the element at index
		Variant.TypeCode		GetTypeCodeAt(int index);

		// gets the value at index, returns undefined if value does not exist
		Variant					GetIndexAsVariant(int index);
		// sets the value at index, this can return a new variant array to support the newly added value
		IVariantArray			SetIndexAsVariant(int index, Variant value);

		// gets the value at index, returns PlayScript.Undefined._undefined if value does not exist
		// note: this function may perform boxing of value types
		object					GetIndexAsUntyped(int index);
		// sets the value at index, this can return a new variant array to support the newly added value
		IVariantArray			SetIndexAsUntyped(int index, object value);

		// clones the variant array, if this array is immutable then the same object can be returned
		IVariantArray			Clone();

		// copies all values into an object array
		// note: this function may perform boxing of value types
		object[]				ToObjectArray();
	}

	// helper base class for all variant arrays
	public abstract class VariantArray : IVariantArray
	{
		// these are implemented in derived classes
		public abstract int 				Length {get;}
		public abstract Variant.TypeCode 	GetTypeCodeAt(int index);
		public abstract Variant 			GetIndexAsVariant(int index);

		public virtual object 			GetIndexAsUntyped(int index)
		{
			return GetIndexAsVariant(index).AsObject();
		}

		// this set index will create a new variant array to accommodate the new value
		public virtual IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			// expand ourselve to become fat
			var newArray = new VariantArrayFull(this, index + 1);
			// set value
			newArray.SetIndexAsVariant(index, value);
			// return new array
			return newArray;
		}

		// this set index will create a new variant array to accommodate the new value
		public virtual IVariantArray SetIndexAsUntyped(int index, object value)
		{
			// expand ourselve to become fat
			var newArray = new VariantArrayFull(this, index + 1);
			// set value
			newArray.SetIndexAsUntyped(index, value);
			// return new array
			return newArray;
		}


		public abstract IVariantArray Clone();

		public virtual object[] ToObjectArray()
		{
			// clone ourselves into an object array
			int length = Length;
			var array = new object[length];
			for (int i=0; i < length; i++) {
				array[i] = GetIndexAsVariant(i).AsObject();
			}
			return array;
		}

		// this creates an array with the best storage by examining the source types
		public static IVariantArray CreateArray(Variant[] source, bool clone)
		{
			if (source.Length == 0) {
				return VariantArrayEmpty.Empty;
			}

			// check array uniformity
			bool uniform = true;
			Variant.TypeCode type = source[0].Type;
			for (int i=1; i < source.Length; i++) {
				if (source[i].Type != type) {
					uniform = false;
					break;
				}
			}

			if (!uniform) {
				// not uniform, just use a full array
				// TODO: create a mixed mode array here
				return new VariantArrayFull(source, clone);
			}

			switch (type)
			{
				case Variant.TypeCode.Int:
					return new IntVariantArray(source);
				case Variant.TypeCode.Number:
					return new NumberVariantArray(source);
				case Variant.TypeCode.Boolean:
					return new BooleanVariantArray(source);
				case Variant.TypeCode.Object:
				case Variant.TypeCode.String:
				case Variant.TypeCode.Null:
					return new UntypedVariantArray(source);
				case Variant.TypeCode.Undefined:
					return VariantArrayEmpty.Empty;
				default:
					throw new Exception();
			}
		}

	}

	// empty variant array, all values undefined
	public class VariantArrayEmpty : VariantArray
	{
		public override int 				Length {get {return 0;}}
		public override Variant.TypeCode 	GetTypeCodeAt(int index) {return Variant.TypeCode.Undefined;}
		public override Variant 			GetIndexAsVariant(int index) {return Variant.Undefined;}
		public override object 				GetIndexAsUntyped(int index) {return PlayScript.Undefined._undefined;}
		public override IVariantArray Clone()
		{
			return Empty;
		}

		public static readonly VariantArrayEmpty Empty = new VariantArrayEmpty();
	}

	// uniform variant array, all values are the same type T
	public abstract class VariantArrayUniform<T> : VariantArray
	{
		public override int Length 
		{
			get {return mData.Length;}
		}

		protected VariantArrayUniform(int length) 
		{
			mData = new T[length];
		}

		protected VariantArrayUniform(T[] source, bool clone) 
		{
			if (clone) {
				mData = new T[source.Length];
				Array.Copy(source, mData, source.Length);
			} else {
				mData = source;
			}
		}

		protected T[] mData;
	};

	// uniform variant array, all values are "*"
	public class UntypedVariantArray : VariantArrayUniform<object>
	{
		public override Variant.TypeCode GetTypeCodeAt(int index)
		{
			object value = mData[index];
			return Variant.GetTypeCodeFromObject(value);
		}

		public override Variant GetIndexAsVariant(int index)
		{
			return Variant.FromAnyType(mData[index]);
		}

		public override IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			mData[index] = value.AsUntyped();
			return this;
		}

		public override object GetIndexAsUntyped(int index)
		{
			return mData[index];
		}

		public override IVariantArray SetIndexAsUntyped(int index, object value)
		{
			mData[index] = value;
			return this;
		}

		public override IVariantArray Clone()
		{
			// clone ourselves
			return new UntypedVariantArray(mData, true);
		}

		public override object[] ToObjectArray()
		{
			return mData;
		}

		public UntypedVariantArray(object[] source, bool clone) 
			: base(source, clone)
		{
		}

		public UntypedVariantArray(Variant[] source) 
			: base(source.Length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source[i].AsUntyped();
			}
		}

		public UntypedVariantArray(IVariantArray source) 
			: base(source.Length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source.GetIndexAsVariant(i).AsUntyped();
			}
		}
	};

	// uniform variant array, all values are int
	public class IntVariantArray : VariantArrayUniform<int>
	{
		public override Variant.TypeCode GetTypeCodeAt(int index)
		{
			return Variant.TypeCode.Int;
		}

		public override Variant GetIndexAsVariant(int index)
		{
			return new Variant(mData[index]);
		}

		public override IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			if (value.Type == Variant.TypeCode.Int) {
				mData[index] = value.AsInt();
				return this;
			} else {
				return base.SetIndexAsVariant(index, value);
			}
		}

		public override IVariantArray Clone()
		{
			// clone ourselves
			return new IntVariantArray(mData, true);
		}

		public IntVariantArray(int[] source, bool clone) 
			: base(source, clone)
		{
		}

		public IntVariantArray(Variant[] source) 
			: base(source.Length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source[i].AsInt();
			}
		}
	};

	// uniform variant array, all values are uint
	public class UIntVariantArray : VariantArrayUniform<uint>
	{
		public override Variant.TypeCode GetTypeCodeAt(int index)
		{
			return Variant.TypeCode.UInt;
		}

		public override Variant GetIndexAsVariant(int index)
		{
			return new Variant(mData[index]);
		}

		public override IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			if (value.Type == Variant.TypeCode.UInt) {
				mData[index] = value.AsUInt();
				return this;
			} else {
				return base.SetIndexAsVariant(index, value);
			}
		}

		public override IVariantArray Clone()
		{
			// clone ourselves
			return new UIntVariantArray(mData, true);
		}

		public UIntVariantArray(uint[] source, bool clone) 
			: base(source, clone)
		{
		}

		public UIntVariantArray(Variant[] source) 
			: base(source.Length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source[i].AsUInt();
			}
		}
	};

	// uniform variant array, all values are number
	public class NumberVariantArray : VariantArrayUniform<double>
	{
		public override Variant.TypeCode GetTypeCodeAt(int index)
		{
			return Variant.TypeCode.Number;
		}

		public override Variant GetIndexAsVariant(int index)
		{
			return new Variant(mData[index]);
		}

		public override IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			if (value.Type == Variant.TypeCode.Number) {
				mData[index] = value.AsNumber();
				return this;
			} else {
				return base.SetIndexAsVariant(index, value);
			}
		}

		public override IVariantArray Clone()
		{
			// clone ourselves
			return new NumberVariantArray(mData, true);
		}

		public NumberVariantArray(double[] source, bool clone) 
			: base(source, clone)
		{
		}

		public NumberVariantArray(Variant[] source) 
			: base(source.Length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source[i].AsNumber();
			}
		}
	};

	// uniform variant array, all values are boolean
	public class BooleanVariantArray : VariantArrayUniform<bool>
	{
		public override Variant.TypeCode GetTypeCodeAt(int index)
		{
			return Variant.TypeCode.Boolean;
		}

		public override Variant GetIndexAsVariant(int index)
		{
			return new Variant(mData[index]);
		}

		public override IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			if (value.Type == Variant.TypeCode.Boolean) {
				mData[index] = value.AsBoolean();
				return this;
			} else {
				return base.SetIndexAsVariant(index, value);
			}
		}

		public override IVariantArray Clone()
		{
			// clone ourselves
			return new BooleanVariantArray(mData, true);
		}

		public BooleanVariantArray(bool[] source, bool clone) 
			: base(source, clone)
		{
		}

		public BooleanVariantArray(Variant[] source) 
			: base(source.Length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source[i].AsBoolean();
			}
		}
	};

	// all values are variant
	// this array must support reading and writing of any variant type
	public class VariantArrayFull : VariantArrayUniform<Variant>
	{
		public override Variant.TypeCode GetTypeCodeAt(int index)
		{
			return mData[index].Type;
		}

		public override Variant GetIndexAsVariant(int index)
		{
			return mData[index];
		}

		public override IVariantArray SetIndexAsVariant(int index, Variant value)
		{
			mData[index] = value;
			return this;
		}

		public override object GetIndexAsUntyped(int index)
		{
			return mData[index].AsObject();
		}

		public override IVariantArray SetIndexAsUntyped(int index, object value)
		{
			mData[index] = Variant.FromAnyType(value);
			return this;
		}

		public override IVariantArray Clone()
		{
			// clone ourselves
			return new VariantArrayFull(mData, true);
		}

		public VariantArrayFull(int length) 
			: base(length)
		{
		}

		public VariantArrayFull(Variant[] source, bool clone) 
			: base(source, clone)
		{
		}

		public VariantArrayFull(IVariantArray source, int length) 
			: base(length)
		{
			for (int i=0; i < source.Length; i++) {
				mData[i] = source.GetIndexAsVariant(i);
			}
		}
	};

}
