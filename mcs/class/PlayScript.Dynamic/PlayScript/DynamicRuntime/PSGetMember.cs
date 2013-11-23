//
// PSGetMember.cs
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
#define USE_IDICTIONARY_STRING_OBJECT

#if !DYNAMIC_SUPPORT
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace PlayScript.DynamicRuntime
{
	public class PSGetMember
	{
		public PSGetMember(string name)
		{
			mName = name;
		}

		public void SetName(string name)
		{
			if (name != mName)
			{
				mName = name;
				mNameHint = 0; // invalidate name hint when name changes
				mType = null;
			}
		}

		public T GetNamedMember<T>(object o, string name)
		{
			SetName(name);
			return GetMemberInternal<T>(o);
		}

		//
		// Note that there are two types of GetMember accessors here: GetMemberTo and GetMemberAs
		//
		// The "To" form is used for explicit casts:
		//      var x:int = int(o.property);
		// The "As" form is used for as casts
		//      var x:int = o.property as int;
		//
		// explicit and as casts have different rules
		//

		public bool GetMemberToBoolean(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberBool(mName, ref mNameHint, false);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToBoolean(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<bool>(o);
		}

		public int GetMemberToInt(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberInt(mName, ref mNameHint, 0);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToInt(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<int>(o);
		}

		public uint GetMemberToUInt(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberUInt(mName, ref mNameHint, 0u);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToUInt(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<uint>(o);
		}

		public float GetMemberToFloat(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return (float)accessor.GetMemberNumber(mName, ref mNameHint, double.NaN);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToFloat(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return (float)GetMemberInternal<double>(o);
		}

		public double GetMemberToNumber(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberNumber(mName, ref mNameHint, double.NaN);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToNumber(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<double>(o);
		}

		public string GetMemberToString(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberString(mName, ref mNameHint, "undefined");
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ToString(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<string>(o);
		}

		public dynamic GetMemberToObject(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberObject(mName, ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetMemberOrDefault(mName, ref mNameHint, null);
			}
			return GetMemberInternal<object>(o);
		}

		[return: AsUntyped]
		public dynamic GetMemberToUntyped(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberUntyped(mName, ref mNameHint, PlayScript.Undefined._undefined);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetMember(mName, ref mNameHint);
			}
			return GetMemberInternal<object>(o);
		}

		public T GetMemberToReference<T>(object o) where T:class
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			// get untyped accessor
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return (T)accessor.GetMemberObject(mName, ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return (T)untypedAccessor.GetMemberOrDefault(mName, ref mNameHint, null);
			}

			// fallback
			return GetMemberInternal<T>(o);
		}

		//
		// AS casts
		//

		public bool GetMemberAsBoolean(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.AsBoolean(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return PSConverter.AsBoolean(GetMemberInternal<object>(o));
		}

		public int GetMemberAsInt(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.AsInt(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return PSConverter.AsInt(GetMemberInternal<object>(o));
		}

		public uint GetMemberAsUInt(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.AsUInt(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return PSConverter.AsUInt(GetMemberInternal<object>(o));
		}

		public float GetMemberAsFloat(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.AsFloat(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return PSConverter.AsFloat(GetMemberInternal<object>(o));
		}

		public double GetMemberAsNumber(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.AsNumber(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return PSConverter.AsNumber(GetMemberInternal<object>(o));
		}

		public string GetMemberAsString(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.AsString(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return PSConverter.AsString(GetMemberInternal<object>(o));
		}

		public dynamic GetMemberAsObject(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetMemberOrDefault(mName, ref mNameHint, null);
			}
			return GetMemberInternal<object>(o);
		}

		[return: AsUntyped]
		public dynamic GetMemberAsUntyped(object o)
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetMember(mName, ref mNameHint);
			}
			return GetMemberInternal<object>(o);
		}

		public T GetMemberAsReference<T>(object o) where T:class
		{
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetMemberOrDefault(mName, ref mNameHint, null) as T;
			}
			return GetMemberInternal<object>(o) as T;
		}

		//
		// These implicit casts happen for PlayScript implicit casts:
		//
		// implicit cast to boolean:
		//  	if (o.boolValue) {}    
		// implicit cast to string:
		// 		switch (o.name) {case "abc":}  
		//

		public bool GetMemberImplicitToBoolean(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberBool(mName, ref mNameHint, false);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToBoolean(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<bool>(o);
		}

		public string GetMemberImplicitToString(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberString(mName, ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return PSConverter.ImplicitToString(untypedAccessor.GetMember(mName, ref mNameHint));
			}
			return GetMemberInternal<string>(o);
		}

		public object GetMemberImplicitToObject(object o)
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return accessor.GetMemberObject(mName, ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return untypedAccessor.GetMemberOrDefault(mName, ref mNameHint, null);
			}
			return GetMemberInternal<object>(o);
		}

		public T GetMemberImplicitToReference<T>(object o) where T:class
		{
			#if USE_DYNAMIC_ACCESSOR_TYPED
			var accessor = o as IDynamicAccessorTyped;
			if (accessor != null) {
				return (T)accessor.GetMemberObject(mName, ref mNameHint, null);
			}
			#endif
			var untypedAccessor = o as IDynamicAccessorUntyped;
			if (untypedAccessor != null) {
				return (T)untypedAccessor.GetMemberOrDefault(mName, ref mNameHint, null);
			}
			return GetMemberInternal<T>(o);
		}

		/// <summary>
		/// This is the most generic method for getting a member's value.
		/// It will attempt to resolve the member by name and the get its value by invoking the 
		/// callsite's delegate
		/// </summary>
		private T GetMemberInternal<T> (object o)
		{
			Stats.Increment(StatsCounter.GetMemberBinderInvoked);

			TypeLogger.LogType(o);

			#if USE_DYNAMIC_ACCESSOR_T
			// get accessor for value type T
			var accessor = o as IDynamicAccessor<T>;
			if (accessor != null) {
				return accessor.GetMember(mName, ref mNameHint);
			}
			#endif

			#if USE_IDICTIONARY_STRING_OBJECT
			// resolve as dictionary (this is usually an expando)
			var dict = o as IDictionary<string, object>;
			if (dict != null) 
			{
				Stats.Increment(StatsCounter.GetMemberBinder_Expando);

				// special case this for expando objects
				object value;
				if (dict.TryGetValue(mName, out value)) {
					// fast path empty cast just in case
					if (value is T) {
						return (T)value;
					} else {
						return PlayScript.Dynamic.ConvertValue<T>(value);
					}
				}

				// key not found
				return Dynamic.GetUndefinedValue<T>();
			}
			#endif

			if (PlayScript.Dynamic.IsNullOrUndefined(o)) {
				return Dynamic.GetUndefinedValue<T>();
			}

			// determine if this is a instance member or a static member
			bool isStatic;
			Type otype;
			if (o is System.Type) {
				// static member
				otype = (System.Type)o;
				o = null;
				isStatic = true;
			} else {
				// instance member
				otype = o.GetType();
				isStatic = false;
			}

			if (otype == mType)
			{
				// use cached resolve
				if (mProperty != null) {
					Func<T> func;
					if (o == mPreviousTarget) {
						func = (Func<T>)mPreviousFunc;
					} else {
						mPreviousFunc = func = ActionCreator.CreatePropertyGetAction<T>(o, mProperty);
						mPreviousTarget = o;
					}
					return func();
				}

				if (mField != null) {
					return PlayScript.Dynamic.ConvertValue<T>(mField.GetValue(o));
				}

				if (mMethod != null) {
					// construct method delegate
					return PlayScript.Dynamic.ConvertValue<T>(Delegate.CreateDelegate(mTargetType, o, mMethod));
				}

				// resolve as dynamic class
				var dc = o as IDynamicClass;
				if (dc != null) 
				{
					object result = dc.__GetDynamicValue(mName);
					return PlayScript.Dynamic.ConvertValue<T>(result);
				}

				if (mName == "constructor") {
					return PlayScript.Dynamic.ConvertValue<T> (otype);
				}

				throw new System.InvalidOperationException("Unhandled member type in PSGetMemberBinder");
			}

			// resolve name
			Stats.Increment(StatsCounter.GetMemberBinder_Resolve_Invoked);

			// The constructor is a special synthetic property - we have to handle this for AS compatibility
			if (mName == "constructor") {
				// setup binding to field
				mType = otype;
				mPreviousFunc = null;
				mPreviousTarget = null;
				mProperty = null;
				mField = null;
				mMethod = null;
				mTargetType = typeof(Type);
				return PlayScript.Dynamic.ConvertValue<T> (otype);
			}

			// resolve as property
			// TODO: we allow access to non-public properties for simplicity,
			// should cleanup to check access levels
			var property = otype.GetProperty(mName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				// found property
				var getter = property.GetGetMethod();
				if (getter != null && getter.IsStatic == isStatic)
				{
					// setup binding to property
					mType     = otype;
					mPreviousFunc = null;
					mPreviousTarget = null;
					mProperty = property;
					mPropertyGetter = property.GetGetMethod();
					mField    = null;
					mMethod   = null;
					mTargetType = property.PropertyType;
					return PlayScript.Dynamic.ConvertValue<T>(mPropertyGetter.Invoke(o, null));
				}
			}

			// resolve as field
			// TODO: we allow access to non-public fields for simplicity,
			// should cleanup to check access levels
			var field = otype.GetField(mName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				// found field
				if (field.IsStatic == isStatic) {
					// setup binding to field
					mType     = otype;
					mPreviousFunc = null;
					mPreviousTarget = null;
					mProperty = null;
					mField    = field;
					mMethod   = null;
					mTargetType = field.FieldType;
					return PlayScript.Dynamic.ConvertValue<T>(field.GetValue(o));
				}
			}

			// resolve as method
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public;
			if (isStatic) {
				flags |= BindingFlags.Static;
			} else {
				flags |= BindingFlags.Instance;
			}
			var method = otype.GetMethod(mName, flags);
			if (method != null)
			{
				// setup binding to method
				mType     = otype;
				mPreviousFunc = null;
				mPreviousTarget = null;
				mProperty = null;
				mField    = null;
				mMethod   = method;
				mTargetType = PlayScript.Dynamic.GetDelegateTypeForMethod(mMethod);
				if (mTargetType == null) {
				}

				// construct method delegate
				return PlayScript.Dynamic.ConvertValue<T>(Delegate.CreateDelegate(mTargetType, o, mMethod));
			}

			if (o is IDynamicClass)
			{
				// dynamic class
				mType     = otype;
				mPreviousFunc = null;
				mPreviousTarget = null;
				mProperty = null;
				mField    = null;
				mMethod   = null;
				object result = ((IDynamicClass)o).__GetDynamicValue(mName);
				return PlayScript.Dynamic.ConvertValue<T>(result);
			}

			return Dynamic.GetUndefinedValue<T>();
		}


		private string			mName;
		private uint 			mNameHint;
		private Type			mType;
		private PropertyInfo	mProperty;
		private FieldInfo		mField;
		private MethodInfo		mMethod;
		private MethodInfo		mPropertyGetter;
		private Type			mTargetType;
		private object			mPreviousTarget;
		private object			mPreviousFunc;
	};
}
#endif
