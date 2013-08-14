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

#if !DYNAMIC_SUPPORT

using System;
using System.Collections;
using System.Collections.Generic;
using PlayScript;
using PlayScript.Expando;

namespace PlayScript.DynamicRuntime
{
	public class PSGetIndex 
	{
		private PSGetMember	mGetMember;
		private int			mHintDictionaryStringID = -1;
		private int			mHintDictionaryIndex = -1;

		public T GetIndexAs<T> (object o, int index)
		{
			Stats.Increment(StatsCounter.GetIndexBinderInvoked);
			Stats.Increment(StatsCounter.GetIndexBinder_Int_Invoked);

			var l = o as IList<T>;
			if (l != null) {
				return l [index];
			}

			var l2 = o as IList;
			if (l2 != null) {
				var ro = l2 [index];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			}

			var d = o as IDictionary<int,T>;
			if (d != null) {
				var ro = d[index];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			}

			var d2 = o as IDictionary;
			if (d2 != null) {
				var ro = d2[index];
				if (ro is T) {
					return (T)ro;
				} else {
					return Dynamic.ConvertValue<T>(ro);
				}
			}

			return default(T);
		}

		public T GetIndexAs<T> (object o, uint index)
		{
			return GetIndexAs<T>(o, (int)index);
		}

		public T GetIndexAs<T> (object o, double index)
		{
			return GetIndexAs<T>(o, (int)index);
		}

		public T GetIndexAs<T> (object o, string key)
		{
			Stats.Increment(StatsCounter.GetIndexBinderInvoked);
			Stats.Increment(StatsCounter.GetIndexBinder_Key_Invoked);

			// First let's look if we can cast to a specific primitive
/*
			IFastDictionaryLookup<T> fastLookupT = o as IFastDictionaryLookup<T>;
			if (fastLookupT != null)
			{
				return fastLookupT.GetValue(key, ref mHintDictionaryStringID, ref mHintDictionaryIndex);
			}

			// If not, T is not a primitive (or object), thus it is probably a class, so use the IFastDictionaryLookup<object> version.
			IFastDictionaryLookup<object> fastLookupObject = o as IFastDictionaryLookup<object>;
			if (fastLookupObject != null)
			{
				object value = fastLookupObject.GetValue(key, ref mHintDictionaryStringID, ref mHintDictionaryIndex);
				if (value is T)
					return (T)value;
				else
					return PlayScript.Dynamic.ConvertValue<T>(value);
			}
*/
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

			// fallback on getmemberbinder to do the hard work 
			Stats.Increment(StatsCounter.GetIndexBinder_Key_Property_Invoked);

			// create a get member binder here
			if (mGetMember == null) {
				mGetMember = new PSGetMember(key);
			}
			
			// get member value
			return mGetMember.GetNamedMember<T>(o, key);			
		}

		
		public T GetIndexAs<T> (object o, object key)
		{
			if (key is int) {
				return GetIndexAs<T>(o, (int)key);
			} else if (key is string) {
				return GetIndexAs<T>(o, (string)key);
			}  else if (key is uint) {
				return GetIndexAs<T>(o, (uint)key);
			}  else if (key is double) {
				return GetIndexAs<T>(o, (double)key);
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

