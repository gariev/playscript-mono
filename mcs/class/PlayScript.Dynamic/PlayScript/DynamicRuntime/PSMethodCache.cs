
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
using System.Reflection;

using PlayScript;

namespace PlayScript.DynamicRuntime
{
	static class PSMethodCache
	{
		public struct PropertyKey
		{
			public PropertyKey(Type type, string name)
			{
				Type = type;
				Name = name;
			}

			public Type	Type;
			public string	Name;
		}

		public class KeyEqualityComparer : IEqualityComparer<PropertyKey>
		{
			public bool Equals (PropertyKey x, PropertyKey y)
			{
				return (x.Name == y.Name) && (x.Type == y.Type);
			}
			public int GetHashCode (PropertyKey key)
			{
				return key.Type.GetHashCode() ^ key.Name.GetHashCode();
			}
		}

		struct PropertyValue
		{
			public MethodInfo	GetMethod;
			public MethodInfo	SetMethod;
			public bool			IsStatic;
		}

		static Dictionary<PropertyKey, PropertyValue> sProperties = new Dictionary<PropertyKey, PropertyValue>(new KeyEqualityComparer());

		public static MethodInfo GetPropertyGet(Type type, string name, bool isStatic)
		{
			PropertyValue value;
			GetPropertyValue(type, name, out value);
			if (value.IsStatic == isStatic)
			{
				return value.GetMethod;
			}
			return null;
		}

		public static MethodInfo GetPropertySet(Type type, string name, bool isStatic)
		{
			PropertyValue value;
			GetPropertyValue(type, name, out value);
			if (value.IsStatic == isStatic)
			{
				return value.SetMethod;
			}
			return null;
		}

		static void GetPropertyValue(Type type, string name, out PropertyValue value)
		{
			PropertyKey key = new PropertyKey(type, name);
			if (sProperties.TryGetValue(key, out value) == false)
			{
				PropertyInfo propertyInfo = type.GetProperty(name);
				if (propertyInfo != null)
				{
					MethodInfo getMethod = propertyInfo.GetGetMethod();
					MethodInfo setMethod = propertyInfo.GetSetMethod();

					if ((getMethod != null) && getMethod.IsPublic)
					{
						value.GetMethod = getMethod;
						value.IsStatic = getMethod.IsStatic;
					}

					if ((setMethod != null) && setMethod.IsPublic)
					{
						value.SetMethod = setMethod;
						value.IsStatic = setMethod.IsStatic;
					}
				}
				sProperties.Add(key, value);
			}
		}
	}
}

