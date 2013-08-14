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
using System.Diagnostics;
using System.Reflection;

namespace PlayScript
{
	/// <summary>
	/// This class is used to provide a string pool, so strings are reused and not reallocated when created dynamically.
	/// For most cases, this is used with expando objects.
	/// </summary>
	internal static class StringPool
	{
		static int sStringID = 0;			// This increments, should we mitigate issues related to wrap around?

		// At some point, we might want to change the implementation to not use a .NET dictionary.
		// There does not seem to be a quick way to get the key in the dictionary when we retrieve the value.
		// In our case, we want the key (as it is the pooled string), but currently that makes us waste 4 bytes of memory in the value to store the same information.
		// Also we might be able to optimize the string lookup by creating some custom code (sharing hash-code).

		class StringInfo
		{
			public string PooledString;
			// We "might" be able to use short instead of int here.
			public int	ID;
			public int	Usage;
		}
		static Dictionary<string, StringInfo> sStringToInfo = new Dictionary<string, StringInfo>(1024);

		public static bool IsStringInPool(string value)
		{
			return sStringToInfo.ContainsKey(value);
		}

		public static bool IsThisStringInPool(string value, int stringID)
		{
			StringInfo info;
			if (sStringToInfo.TryGetValue(value, out info))
			{
				Debug.Assert(stringID == info.ID);		// Make sure the ID is valid too
				return object.ReferenceEquals(info.PooledString, value);
			}
			return false;
		}

		public static int GetStringID(string value)
		{
			StringInfo info;
			if (sStringToInfo.TryGetValue(value, out info))
			{
				return info.ID;
			}
			return -1;
		}

		public static string AddStringToPool(string value, out int stringID)
		{
			Stats.Increment(StatsCounter.StringPool_Add);

			StringInfo info;
			if (sStringToInfo.TryGetValue(value, out info) == false)
			{
				Stats.Increment(StatsCounter.StringPool_AddFirst);

				info = new StringInfo();
				info.ID = sStringID++;
				string internedString = string.IsInterned(value);
				if (internedString != null)
				{
					// If the string is actually interned, let's re-use it
					value = internedString;
					// And we increment the counter twice (to 2 at the end of this function), as there is not much point to release later the StringInfo
					// and recreate it later when it the string is already in memory anyway (interned).
					info.Usage = 1;		
				}
				info.PooledString = value;
				sStringToInfo.Add(value, info);
			}
			stringID = info.ID;
			info.Usage++;
			return info.PooledString;
		}

		public static void ReleaseStringFromPool(string value)
		{
			Stats.Increment(StatsCounter.StringPool_Remove);

			StringInfo info;
			if (sStringToInfo.TryGetValue(value, out info) == false)
			{
#if _DEBUG
				throw new InvalidOperationException();		// Bug in the code, the string was not added to the string pool to begin with
															// Or incorrect ref count somewhere...
#endif
				return;
			}

			info.Usage--;
			if (info.Usage == 0)
			{
				Stats.Increment(StatsCounter.StringPool_RemoveLast);

				// That was the last reference, we can remove it from the pool
				sStringToInfo.Remove(value);
			}
		}
	}
}

