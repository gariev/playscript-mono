//
// PSGetIndex.cs
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

#define USE_DYNAMIC_ACCESSOR_TYPED
#define USE_DYNAMIC_ACCESSOR_T
#define USE_ILIST_T
#define USE_ILIST
#define USE_IDICTIONARY_T
#define USE_IDICTIONARY

#if !DYNAMIC_SUPPORT

using System;
using System.Collections;
using System.Collections.Generic;
using PlayScript;

namespace PlayScript.DynamicRuntime
{
	public class PSGetIndex
	{
		private PSGetMember			  mGetMember;
		#if USE_DYNAMIC_ACCESSOR_TYPED
		private uint mNameHint;
		#endif

		//
		// get index with string keys with explicit casting rules
		// 

		#region String keys, explicit casting

		public bool GetIndexToBoolean(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberBool(Dynamic.ConvertKey(index), ref mNameHint, false);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToBoolean(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<bool>(o, index);
		}

		public bool GetIndexImplicitToBoolean(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberBool(Dynamic.ConvertKey(index), ref mNameHint, false);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToBoolean(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<bool>(o, index);
		}

		public int GetIndexToInt(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberInt(Dynamic.ConvertKey(index), ref mNameHint, 0);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToInt(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<int>(o, index);
		}

		public uint GetIndexToUInt(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberUInt(Dynamic.ConvertKey(index), ref mNameHint, 0u);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToUInt(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<uint>(o, index);
		}

		public double GetIndexToNumber(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberNumber(Dynamic.ConvertKey(index), ref mNameHint, double.NaN);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToNumber(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<double>(o, index);
		}

		public float GetIndexToFloat(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return (float)accessor.GetMemberNumber(Dynamic.ConvertKey(index), ref mNameHint, double.NaN);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToFloat(untypedAccessor.GetIndex(index));
			}

			return InternalGetIndexTo<float>(o, index);
		}

		public string GetIndexToString(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberString(Dynamic.ConvertKey(index), ref mNameHint, "undefined");
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToString(untypedAccessor.GetIndex(index));
			}

			return InternalGetIndexTo<string>(o, index);
		}

		public string GetIndexImplicitToString(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberString(Dynamic.ConvertKey(index), ref mNameHint, "undefined");
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToString(untypedAccessor.GetIndex(index));
			}

			return InternalGetIndexTo<string>(o, index);
		}

		public dynamic GetIndexToObject(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberObject(Dynamic.ConvertKey(index), ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public dynamic GetIndexImplicitToObject(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberObject(Dynamic.ConvertKey(index), ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.UntypedImplicitToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<object>(o, index);
		}


		[return:AsUntyped]
		public dynamic GetIndexToUntyped(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberUntyped(Dynamic.ConvertKey(index), ref mNameHint, PlayScript.Undefined._undefined);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index);
			}
			return InternalGetIndexTo<object>(o, index);
		}


		public T GetIndexToReference<T>(object o, string index) where T:class
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return (T)accessor.GetMemberObject(Dynamic.ConvertKey(index), ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return (T)PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<T>(o, index);
		}

		public T GetIndexImplicitToReference<T>(object o, string index) where T:class
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return (T)accessor.GetMemberObject(Dynamic.ConvertKey(index), ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return (T)PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<T>(o, index);
		}

		public Variant GetIndexToVariant(object o, string index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var variantAccessor = o as IDynamicAccessor<Variant>;
			if (variantAccessor != null) {
				return variantAccessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return Variant.FromAnyType(untypedAccessor.GetIndex(index));
			}
			return Variant.FromAnyType(InternalGetIndexTo<object>(o, index));
		}

		#endregion

		//
		// get index with int keys with explicit casting rules
		// 

		#region Int keys, explicit casting

		public bool GetIndexToBoolean(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<bool>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToBoolean(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<bool>(o, index);
		}

		public bool GetIndexImplicitToBoolean(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<bool>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToBoolean(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<bool>(o, index);
		}

		public int GetIndexToInt(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<int>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToInt(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<int>(o, index);
		}

		public uint GetIndexToUInt(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<uint>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToUInt(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<uint>(o, index);
		}

		public double GetIndexToNumber(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<double>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToNumber(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<double>(o, index);
		}

		public float GetIndexToFloat(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<float>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToFloat(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<float>(o, index);
		}

		public string GetIndexToString(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<string>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToString(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<string>(o, index);
		}

		public string GetIndexImplicitToString(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<string>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToString(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<string>(o, index);
		}

		public dynamic GetIndexToObject(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<object>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public dynamic GetIndexImplicitToObject(object o, int index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<object>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.UntypedImplicitToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<object>(o, index);
		}

		[return:AsUntyped]
		public dynamic GetIndexToUntyped(object o, int index)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index);
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public T GetIndexToReference<T>(object o, int index) where T:class
		{
			return (T)GetIndexToObject(o, index);
		}

		public T GetIndexImplicitToReference<T>(object o, int index) where T:class
		{
			return (T)GetIndexToObject(o, index);
		}

		#endregion


		#region Object keys, explicit casting

		//
		// get index with object keys with explicit casting rules
		// 
		
		public bool GetIndexToBoolean(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<bool>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToBoolean(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<bool>(o, index);
		}

		public bool GetIndexImplicitToBoolean(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<bool>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToBoolean(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<bool>(o, index);
		}

		public int GetIndexToInt(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<int>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToInt(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<int>(o, index);
		}

		public uint GetIndexToUInt(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<uint>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToUInt(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<uint>(o, index);
		}

		public double GetIndexToNumber(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<double>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToNumber(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<double>(o, index);
		}

		public float GetIndexToFloat(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<float>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToFloat(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<float>(o, index);
		}

		public string GetIndexToString(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<string>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToString(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<string>(o, index);
		}

		public string GetIndexImplicitToString(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<string>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToString(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<string>(o, index);
		}

		public dynamic GetIndexToObject(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<object>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public dynamic GetIndexImplicitToObject(object o, object index)
		{
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<object>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.UntypedImplicitToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<object>(o, index);
		}

		[return:AsUntyped]
		public dynamic GetIndexToUntyped(object o, object index)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index);
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public T GetIndexToReference<T>(object o, object index) where T:class
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return (T)PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<T>(o, index);
		}

		public T GetIndexImplicitToReference<T>(object o, object index) where T:class
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return (T)PSConverter.UntypedToObject(untypedAccessor.GetIndex(index));
			}
			return InternalGetIndexTo<T>(o, index);
		}

		#endregion

		//
		// Get index with "as" cast operator
		//


		#region String keys, explicit casting


		//
		// string keys, as cast
		//
		
		public bool GetIndexAsBoolean(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsBoolean(GetIndexAsUntyped(o, index));
		}

		public int GetIndexAsInt(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsInt(GetIndexAsUntyped(o, index));
		}

		public uint GetIndexAsUInt(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsUInt(GetIndexAsUntyped(o, index));
		}

		public double GetIndexAsNumber(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsNumber(GetIndexAsUntyped(o, index));
		}

		public float GetIndexAsFloat(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsFloat(GetIndexAsUntyped(o, index));
		}

		public string GetIndexAsString(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsString(GetIndexAsUntyped(o, index));
		}

		public dynamic GetIndexAsObject(object o, string index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.UntypedAsObject(GetIndexAsUntyped(o, index));
		}

		[return:AsUntyped]
		public dynamic GetIndexAsUntyped(object o, string index)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index);
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public T GetIndexAsReference<T>(object o, string index) where T:class
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index) as T;
			}
			return InternalGetIndexTo<object>(o, index) as T;
		}
		#endregion

		#region Int keys, as casting

		//
		// int keys, as cast
		//

		public bool GetIndexAsBoolean(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsBoolean(GetIndexAsUntyped(o, index));
		}

		public int GetIndexAsInt(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsInt(GetIndexAsUntyped(o, index));
		}

		public uint GetIndexAsUInt(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsUInt(GetIndexAsUntyped(o, index));
		}

		public double GetIndexAsNumber(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsNumber(GetIndexAsUntyped(o, index));
		}

		public float GetIndexAsFloat(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsFloat(GetIndexAsUntyped(o, index));
		}

		public string GetIndexAsString(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsString(GetIndexAsUntyped(o, index));
		}

		public dynamic GetIndexAsObject(object o, int index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.UntypedAsObject(GetIndexAsUntyped(o, index));
		}

		[return:AsUntyped]
		public dynamic GetIndexAsUntyped(object o, int index)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index);
			}
			return InternalGetIndexTo<object>(o, index);
		}

		public T GetIndexAsReference<T>(object o, int index) where T:class
		{
			return GetIndexAsUntyped(o, index) as T;
		}
		#endregion

		#region Object keys, as casting

		//
		// object keys, as cast
		//

		public bool GetIndexAsBoolean(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsBoolean(GetIndexAsUntyped(o, index));
		}

		public int GetIndexAsInt(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsInt(GetIndexAsUntyped(o, index));
		}

		public uint GetIndexAsUInt(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsUInt(GetIndexAsUntyped(o, index));
		}

		public double GetIndexAsNumber(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsNumber(GetIndexAsUntyped(o, index));
		}

		public float GetIndexAsFloat(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsFloat(GetIndexAsUntyped(o, index));
		}

		public string GetIndexAsString(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.AsString(GetIndexAsUntyped(o, index));
		}

		public dynamic GetIndexAsObject(object o, object index)
		{
			// TODO: introduce interface for this particular cast
			return PSConverter.UntypedAsObject(GetIndexAsUntyped(o, index));
		}

		[return:AsUntyped]
		public dynamic GetIndexAsUntyped(object o, object index)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetIndex(index);
			}
			return InternalGetIndexTo<object>(o, index);		
		}

		public T GetIndexAsReference<T>(object o, object index) where T:class
		{
			return GetIndexAsUntyped(o, index) as T;
		}

		#endregion

		public int ConvertIndex(object o, uint index)
		{
			return (int)index;
		}

		public int ConvertIndex(object o, long index)
		{
			return (int)index;
		}

		public int ConvertIndex(object o, float index)
		{
			return (int)index;
		}

		public int ConvertIndex(object o, double index)
		{
			return (int)index;
		}

		//
		//
		//


		private T InternalGetIndexTo<T> (object o, int index)
		{
			Stats.Increment(StatsCounter.GetIndexBinderInvoked);
			Stats.Increment(StatsCounter.GetIndexBinder_Int_Invoked);

			#if USE_DYNAMIC_ACCESSOR_T
			// get accessor for value type T
			var accessor = o as IDynamicAccessor<T>;
			if (accessor != null) {
				return accessor.GetIndex(index);
			}
			#endif

			// fallback on object accessor and cast it to T
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				object value = untypedAccessor.GetIndex(index);
				// convert value to T
				if (value == null) {
					return default(T);
				} else if (value is T) {
					return (T)value;
				} else if (Dynamic.IsUndefined(value)) {
					return Dynamic.GetUndefinedValue<T>();
				} else {
					return PlayScript.Dynamic.ConvertValue<T>(value);
				}
			}

			#if USE_ILIST_T
			var l = o as IList<T>;
			if (l != null) {
				return l [index];
			}
			#endif

			#if USE_ILIST
			var l2 = o as IList;
			if (l2 != null) {
				if (index >= l2.Count)
					return default(T);
				var ro = l2 [index];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			}
			#endif

			#if USE_IDICTIONARY_T
			var d = o as IDictionary<int,T>;
			if (d != null) {
				var ro = d[index];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			}
			#endif

			#if USE_IDICTIONARY
			var d2 = o as IDictionary;
			if (d2 != null) {
				var ro = d2[index];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			}
			#endif

			return Dynamic.GetUndefinedValue<T>();
		}

		private T InternalGetIndexTo<T> (object o, string key)
		{
			Stats.Increment(StatsCounter.GetIndexBinderInvoked);
			Stats.Increment(StatsCounter.GetIndexBinder_Key_Invoked);

			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<T>;
			if (accessor != null) {
				return accessor.GetIndex(key);
			}
			#endif

			// fallback on object accessor and cast it to T
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				object value = untypedAccessor.GetIndex(key);
				if (value == null) return default(T);
				if (value is T) {
					return (T)value;
				} else {
					return PlayScript.Dynamic.ConvertValue<T>(value);
				}
			}

			#if USE_IDICTIONARY
			// handle dictionaries
			var dict = o as IDictionary;
			if (dict != null) {
				Stats.Increment(StatsCounter.GetIndexBinder_Key_Dictionary_Invoked);

				var ro = dict[key];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			} 
			#endif

			// fallback on getmemberbinder to do the hard work 
			Stats.Increment(StatsCounter.GetIndexBinder_Key_Property_Invoked);

			// create a get member binder here
			if (mGetMember == null) {
				mGetMember = new PSGetMember(key);
			}
			
			// get member value
			return mGetMember.GetNamedMember<T>(o, key);			
		}
		
		public T InternalGetIndexTo<T> (object o, object key)
		{
			key = PlayScript.Dynamic.FormatKeyForAs (key);
			if (key is int) {
				return InternalGetIndexTo<T>(o, (int)key);
			} else if (key is string) {
				return InternalGetIndexTo<T>(o, (string)key);
			}  else if (key is uint) {
				return InternalGetIndexTo<T>(o, (int)(uint)key);
			}  else if (key is double) {
				return InternalGetIndexTo<T>(o, (int)(double)key);
			}  else if (key is float) {
				return InternalGetIndexTo<T>(o, (int)(float)key);
			} else {
				throw new InvalidOperationException("Cannot index object with key of type: " + key.GetType());
			}
		}


		public PSGetIndex()
		{
		}
	}
}
#endif

