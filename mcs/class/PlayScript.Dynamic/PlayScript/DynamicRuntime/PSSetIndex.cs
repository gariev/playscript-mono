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
	public class PSSetIndex
	{

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


		// TODO: now that we can specialize each function we can query for an typed indexer and use the specialized setters

		public bool SetIndexToBoolean(object o, int index, bool value)
		{
			SetIndexTo<bool>(o, index, value);
			return value;
		}

		public bool SetIndexToBoolean(object o, string index, bool value)
		{
			SetIndexTo<bool>(o, index, value);
			return value;
		}

		public bool SetIndexToBoolean(object o, object index, bool value)
		{
			SetIndexTo<bool>(o, index, value);
			return value;
		}

		public int SetIndexToInt(object o, int index, int value)
		{
			SetIndexTo<int>(o, index, value);
			return value;
		}

		public int SetIndexToInt(object o, string index, int value)
		{
			SetIndexTo<int>(o, index, value);
			return value;
		}

		public int SetIndexToInt(object o, object index, int value)
		{
			SetIndexTo<int>(o, index, value);
			return value;
		}

		public uint SetIndexToUInt(object o, int index, uint value)
		{
			SetIndexTo<uint>(o, index, value);
			return value;
		}

		public uint SetIndexToUInt(object o, string index, uint value)
		{
			SetIndexTo<uint>(o, index, value);
			return value;
		}

		public uint SetIndexToUInt(object o, object index, uint value)
		{
			SetIndexTo<uint>(o, index, value);
			return value;
		}

		public double SetIndexToNumber(object o, int index, double value)
		{
			SetIndexTo<double>(o, index, value);
			return value;
		}

		public double SetIndexToNumber(object o, string index, double value)
		{
			SetIndexTo<double>(o, index, value);
			return value;
		}

		public double SetIndexToNumber(object o, object index, double value)
		{
			SetIndexTo<double>(o, index, value);
			return value;
		}

		public float SetIndexToFloat(object o, int index, float value)
		{
			SetIndexTo<float>(o, index, value);
			return value;
		}

		public float SetIndexToFloat(object o, string index, float value)
		{
			SetIndexTo<float>(o, index, value);
			return value;
		}

		public float SetIndexToFloat(object o, object index, float value)
		{
			SetIndexTo<float>(o, index, value);
			return value;
		}

		public string SetIndexToString(object o, int index, string value)
		{
			SetIndexTo<string>(o, index, value);
			return value;
		}

		public string SetIndexToString(object o, string index, string value)
		{
			SetIndexTo<string>(o, index, value);
			return value;
		}

		public string SetIndexToString(object o, object index, string value)
		{
			SetIndexTo<string>(o, index, value);
			return value;
		}

		public dynamic SetIndexToObject(object o, int index, object value)
		{
			SetIndexTo<object>(o, index, value);
			return value;
		}

		public dynamic SetIndexToObject(object o, string index, object value)
		{
			SetIndexTo<object>(o, index, value);
			return value;
		}

		public dynamic SetIndexToObject(object o, object index, object value)
		{
			SetIndexTo<object>(o, index, value);
			return value;
		}

		public T SetIndexToReference<T>(object o, int index, T value)
		{
			SetIndexTo<T>(o, index, value);
			return value;
		}

		public T SetIndexToReference<T>(object o, string index, T value)
		{
			SetIndexTo<T>(o, index, value);
			return value;
		}

		public T SetIndexToReference<T>(object o, object index, T value)
		{
			SetIndexTo<T>(o, index, value);
			return value;
		}



		private PSSetMember mSetMember;

		private void SetIndexTo<T> (object o, int index, T value)
		{
			Stats.Increment(StatsCounter.SetIndexBinderInvoked);
			Stats.Increment(StatsCounter.SetIndexBinder_Int_Invoked);

			TypeLogger.LogType(o);

			// get accessor for value type T
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<T>;
			if (accessor != null) {
				accessor.SetIndex(index, value);
				return;
			}
			#endif

			// fallback on untyped accessor
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				untypedAccessor.SetIndex(index, (object)value);
				return;
			}


			#if USE_ILIST_T
			var l = o as IList<T>;
			if (l != null) {
				l [index] = value;
				return;
			} 
			#endif


			#if USE_ILIST
			var l2 = o as IList;
			if (l2 != null) {
				int count = l2.Count;
				if (index < count)
					l2 [index] = value;
				else if (index == count)
					l2.Add (value);
				else {
					while (l2.Count < index) {
						l2.Add (default(T));
					}
					l2 [index] = value;
				}
				return;
			} 
			#endif

			#if USE_IDICTIONARY_T
			var d = o as IDictionary<int,T>;
			if (d != null) {
				d[index] = value;
				return;
			} 
			#endif

			#if USE_IDICTIONARY
			var d2 = o as IDictionary;
			if (d2 != null) {
				d2[index] = value;
				return;
			}
			#endif
		}

		private void SetIndexTo<T> (object o, string key, T value)
		{
			Stats.Increment(StatsCounter.SetIndexBinderInvoked);
			Stats.Increment(StatsCounter.SetIndexBinder_Key_Invoked);

			// get accessor for value type T
			#if USE_DYNAMIC_ACCESSOR_T
			var accessor = o as IDynamicAccessor<T>;
			if (accessor != null) {
				accessor.SetIndex(key, value);
				return;
			}
			#endif

			// fallback on untyped accessor
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				untypedAccessor.SetIndex(key, (object)value);
				return;
			}

			#if USE_IDICTIONARY
			// handle dictionaries
			var dict = o as IDictionary;
			if (dict != null) {
				Stats.Increment(StatsCounter.SetIndexBinder_Key_Dictionary_Invoked);

				dict[key] = (object)value;
				return;
			} 
			#endif

			// fallback on setmemberbinder to do the hard work 
			Stats.Increment(StatsCounter.SetIndexBinder_Key_Property_Invoked);

			// create a set member binder here to set
			if (mSetMember == null) {
				mSetMember   = new PSSetMember(key);
			}

			// set member value
			mSetMember.SetNamedMember(o, key, value);	
		}

		private void SetIndexTo<T> (object o, object key, T value)
		{
			key = PlayScript.Dynamic.FormatKeyForAs (key);
			if (key is int) {
				SetIndexTo<T>(o, (int)key, value);
			} else if (key is string) {
				SetIndexTo<T>(o, (string)key, value);
			} else  if (key is uint) {
				SetIndexTo<T>(o, ConvertIndex(o, (uint)key), value);
			} else  if (key is double) {
				SetIndexTo<T>(o, ConvertIndex(o, (double)key), value);
			} else  if (key is float) {
				SetIndexTo<T>(o, ConvertIndex(o, (float)key), value);
			} else {
				throw new InvalidOperationException("Cannot index object with key of type: " + key.GetType());
			}
		}

		public PSSetIndex()
		{
		}

	}
}
#endif

