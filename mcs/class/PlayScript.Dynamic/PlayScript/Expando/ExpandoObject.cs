/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#define USE_NEW_EXPANDO

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using PlayScript;

namespace PlayScript.Expando {

	interface IFastDictionaryLookup<T>
	{
		/// <summary>
		/// Gets the value for a key.
		/// </summary>
		/// <returns>The value that was associated with the key.</returns>
		/// <param name="key">The key that we are looking for.</param>
		/// <param name="stringID">The string ID matching the key - Used as a hint. -1 if not known (in and out).</param>
		/// <param name="expandoIndex">The expando index matching the key - Used as a hint. -1 if not known (in and out).</param>
		T GetValue(string key, ref int hintStringID, ref int hintExpandoIndex);

		/// <summary>
		/// Sets the value for a key.
		/// </summary>
		/// <param name="key">The key that we are looking for.</param>
		/// <param name="value">The string ID matching the key - Used as a hint. -1 if not known (in and out).</param>
		/// <param name="stringID">String I.</param>
		/// <param name="expandoIndex">The expando index matching the key - Used as a hint. -1 if not known (in and out).</param>
		void SetValue(string key, T value, ref int hintStringID, ref int hintExpandoIndex);
	}



#if !USE_NEW_EXPANDO


	/* 
	 * Declare this outside the main class so it doesn't have to be inflated for each
	 * instantiation of Dictionary.
	 */
	internal struct Link {
		public int HashCode;
		public int Next;
	}

	[ComVisible(false)]
	[Serializable]
	[DebuggerDisplay ("Count = {Count}")]
	[DebuggerTypeProxy (typeof (ExpandoDebugView))]
	public class ExpandoObject : IDictionary<string, object>, IDictionary, ISerializable, IDeserializationCallback, IDynamicClass, IKeyEnumerable,
		IFastDictionaryLookup<int>, IFastDictionaryLookup<uint>, IFastDictionaryLookup<double>, IFastDictionaryLookup<bool>, IFastDictionaryLookup<string>, IFastDictionaryLookup<object>
#if NET_4_5
		, IReadOnlyDictionary<string, object>
#endif
	{
		// The implementation of this class uses a hash table and linked lists
		// (see: http://msdn2.microsoft.com/en-us/library/ms379571(VS.80).aspx).
		//		
		// We use a kind of "mini-heap" instead of reference-based linked lists:
		// "keySlots" and "valueSlots" is the heap itself, it stores the data
		// "linkSlots" contains information about how the slots in the heap
		//             are connected into linked lists
		//             In addition, the HashCode field can be used to check if the
		//             corresponding key and value are present (HashCode has the
		//             HASH_FLAG bit set in this case), so, to iterate over all the
		//             items in the dictionary, simply iterate the linkSlots array
		//             and check for the HASH_FLAG bit in the HashCode field.
		//             For this reason, each time a hashcode is calculated, it needs
		//             to be ORed with HASH_FLAG before comparing it with the save hashcode.
		// "touchedSlots" and "emptySlot" manage the free space in the heap 
		
		const int INITIAL_SIZE = 10;
		const float DEFAULT_LOAD_FACTOR = (90f / 100);
		const int NO_SLOT = -1;
		const int HASH_FLAG = -2147483648;
		
		// The hash table contains indices into the linkSlots array
		int [] table;
		
		// All (key,value) pairs are chained into linked lists. The connection
		// information is stored in "linkSlots" along with the key's hash code
		// (for performance reasons).
		// TODO: get rid of the hash code in Link (this depends on a few
		// JIT-compiler optimizations)
		// Every link in "linkSlots" corresponds to the (key,value) pair
		// in "keySlots"/"valueSlots" with the same index.
		Link [] linkSlots;
		string [] keySlots;
		object [] valueSlots;
		
		//Leave those 2 fields here to improve heap layout.
		IEqualityComparer<string> hcp;
		SerializationInfo serialization_info;
		
		// The number of slots in "linkSlots" and "keySlots"/"valueSlots" that
		// are in use (i.e. filled with data) or have been used and marked as
		// "empty" later on.
		int touchedSlots;
		
		// The index of the first slot in the "empty slots chain".
		// "Remove()" prepends the cleared slots to the empty chain.
		// "Add()" fills the first slot in the empty slots chain with the
		// added item (or increases "touchedSlots" if the chain itself is empty).
		int emptySlot;
		
		// The number of (key,value) pairs in this dictionary.
		int count;
		
		// The number of (key,value) pairs the dictionary can hold without
		// resizing the hash table and the slots arrays.
		int threshold;
		
		// The number of changes made to this dictionary. Used by enumerators
		// to detect changes and invalidate themselves.
		int generation;
		
		public int Count {
			get { return count; }
		}

		public int Generation {
			get { return generation; }
		}
		
		public dynamic this [string key] {
			get {
				if (key == null)
					throw new ArgumentNullException ("key");
				
				// get first item of linked list corresponding to given key
				int hashCode = hcp.GetHashCode (key) | HASH_FLAG;
				int cur = table [(hashCode & int.MaxValue) % table.Length] - 1;
				
				// walk linked list until right slot is found or end is reached 
				while (cur != NO_SLOT) {
					// The ordering is important for compatibility with MS and strange
					// Object.Equals () implementations
					if (linkSlots [cur].HashCode == hashCode && hcp.Equals (keySlots [cur], key))
						return valueSlots [cur];
					cur = linkSlots [cur].Next;
				}
				// this is not an exceptional condition although we should be returning undefined instead of null
				return null;
				//throw new KeyNotFoundException ();
			}
			
			set {
				if (key == null)
					throw new ArgumentNullException ("key");
				
				// get first item of linked list corresponding to given key
				int hashCode = hcp.GetHashCode (key) | HASH_FLAG;
				int index = (hashCode & int.MaxValue) % table.Length;
				int cur = table [index] - 1;
				
				// walk linked list until right slot (and its predecessor) is
				// found or end is reached
				int prev = NO_SLOT;
				if (cur != NO_SLOT) {
					do {
						// The ordering is important for compatibility with MS and strange
						// Object.Equals () implementations
						if (linkSlots [cur].HashCode == hashCode && hcp.Equals (keySlots [cur], key))
							break;
						prev = cur;
						cur = linkSlots [cur].Next;
					} while (cur != NO_SLOT);
				}
				
				// is there no slot for the given key yet? 				
				if (cur == NO_SLOT) {
					// there is no existing slot for the given key,
					// allocate one and prepend it to its corresponding linked
					// list
					
					if (++count > threshold) {
						Resize ();
						index = (hashCode & int.MaxValue) % table.Length;
					}
					
					// find an empty slot
					cur = emptySlot;
					if (cur == NO_SLOT)
						cur = touchedSlots++;
					else 
						emptySlot = linkSlots [cur].Next;
					
					// prepend the added item to its linked list,
					// update the hash table
					linkSlots [cur].Next = table [index] - 1;
					table [index] = cur + 1;
					
					// store the new item and its hash code
					linkSlots [cur].HashCode = hashCode;
					keySlots [cur] = key;
				} else {
					// we already have a slot for the given key,
					// update the existing slot		
					
					// if the slot is not at the front of its linked list,
					// we move it there
					if (prev != NO_SLOT) {
						linkSlots [prev].Next = linkSlots [cur].Next;
						linkSlots [cur].Next = table [index] - 1;
						table [index] = cur + 1;
					}
				}
				
				// store the item's data itself
				valueSlots [cur] = value;
				
				generation++;
			}
		}
		
		public ExpandoObject ()
		{
			Init (INITIAL_SIZE, null);
		}
		
		public ExpandoObject (IEqualityComparer<string> comparer)
		{
			Init (INITIAL_SIZE, comparer);
		}
		
		public ExpandoObject (IDictionary<string, object> dictionary)
			: this (dictionary, null)
		{
		}
		
		public ExpandoObject (int capacity)
		{
			Init (capacity, null);
		}
		
		public ExpandoObject (IDictionary<string, object> dictionary, IEqualityComparer<string> comparer)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			int capacity = dictionary.Count;
			Init (capacity, comparer);
			foreach (KeyValuePair<string, object> entry in dictionary)
				this.Add (entry.Key, entry.Value);
		}
		
		public ExpandoObject (int capacity, IEqualityComparer<string> comparer)
		{
			Init (capacity, comparer);
		}
		
		protected ExpandoObject (SerializationInfo info, StreamingContext context)
		{
			serialization_info = info;
		}
		
		private void Init (int capacity, IEqualityComparer<string> hcp)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			this.hcp = (hcp != null) ? hcp : EqualityComparer<string>.Default;
			if (capacity == 0)
				capacity = INITIAL_SIZE;
			
			/* Modify capacity so 'capacity' elements can be added without resizing */
			capacity = (int)(capacity / DEFAULT_LOAD_FACTOR) + 1;
			
			InitArrays (capacity);
			generation = 0;
		}
		
		private void InitArrays (int size) {
			table = new int [size];
			
			linkSlots = new Link [size];
			emptySlot = NO_SLOT;
			
			keySlots = new string [size];
			valueSlots = new object [size];
			touchedSlots = 0;
			
			threshold = (int)(table.Length * DEFAULT_LOAD_FACTOR);
			if (threshold == 0 && table.Length > 0)
				threshold = 1;
		}
		
		void CopyToCheck (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			// we want no exception for index==array.Length && Count == 0
			if (index > array.Length)
				throw new ArgumentException ("index larger than largest valid index of array");
			if (array.Length - index < Count)
				throw new ArgumentException ("Destination array cannot hold the requested elements!");
		}
		
		void CopyKeys (string[] array, int index)
		{
			for (int i = 0; i < touchedSlots; i++) {
				if ((linkSlots [i].HashCode & HASH_FLAG) != 0)
					array [index++] = keySlots [i];
			}
		}
		
		void CopyValues (object[] array, int index)
		{
			for (int i = 0; i < touchedSlots; i++) {
				if ((linkSlots [i].HashCode & HASH_FLAG) != 0)
					array [index++] = valueSlots [i];
			}
		}
		
		delegate TRet Transform<TRet> (string key, object value);
		
		
		static KeyValuePair<string, object> make_pair (string key, object value)
		{
			return new KeyValuePair<string, object> (key, value);
		}
		
		static string pick_key (string key, object value)
		{
			return key;
		}
		
		static object pick_value (string key, object value)
		{
			return value;
		}
		
		void CopyTo (KeyValuePair<string, object> [] array, int index)
		{
			CopyToCheck (array, index);
			for (int i = 0; i < touchedSlots; i++) {
				if ((linkSlots [i].HashCode & HASH_FLAG) != 0)
					array [index++] = new KeyValuePair<string, object> (keySlots [i], valueSlots [i]);
			}
		}
		
		void Do_ICollectionCopyTo<TRet> (Array array, int index, Transform<TRet> transform)
		{
			Type src = typeof (TRet);
			Type tgt = array.GetType ().GetElementType ();
			
			try {
				if ((src.IsPrimitive || tgt.IsPrimitive) && !tgt.IsAssignableFrom (src))
					throw new Exception (); // we don't care.  it'll get transformed to an ArgumentException below
				
				object[] dest = (object[])array;
				for (int i = 0; i < touchedSlots; i++) {
					if ((linkSlots [i].HashCode & HASH_FLAG) != 0)
						dest [index++] = transform (keySlots [i], valueSlots [i]);
				}

			} catch (Exception e) {
				throw new ArgumentException ("Cannot copy source collection elements to destination array", "array", e);
			}
		}
		
		private void Resize ()
		{
			// From the SDK docs:
			//	 Hashtable is automatically increased
			//	 to the smallest prime number that is larger
			//	 than twice the current number of Hashtable buckets
			int newSize = PlayScript.Expando.Hashtable.ToPrime ((table.Length << 1) | 1);
			
			// allocate new hash table and link slots array
			int [] newTable = new int [newSize];
			Link [] newLinkSlots = new Link [newSize];
			
			for (int i = 0; i < table.Length; i++) {
				int cur = table [i] - 1;
				while (cur != NO_SLOT) {
					int hashCode = newLinkSlots [cur].HashCode = hcp.GetHashCode(keySlots [cur]) | HASH_FLAG;
					int index = (hashCode & int.MaxValue) % newSize;
					newLinkSlots [cur].Next = newTable [index] - 1;
					newTable [index] = cur + 1;
					cur = linkSlots [cur].Next;
				}
			}
			table = newTable;
			linkSlots = newLinkSlots;
			
			// allocate new data slots, copy data
			string [] newKeySlots = new string [newSize];
			object [] newValueSlots = new object [newSize];
			Array.Copy (keySlots, 0, newKeySlots, 0, touchedSlots);
			Array.Copy (valueSlots, 0, newValueSlots, 0, touchedSlots);
			keySlots = newKeySlots;
			valueSlots = newValueSlots;			
			
			threshold = (int)(newSize * DEFAULT_LOAD_FACTOR);
		}
		
		public void Add (string key, object value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			// get first item of linked list corresponding to given key
			int hashCode = hcp.GetHashCode (key) | HASH_FLAG;
			int index = (hashCode & int.MaxValue) % table.Length;
			int cur = table [index] - 1;
			
			// walk linked list until end is reached (throw an exception if a
			// existing slot is found having an equivalent key)
			while (cur != NO_SLOT) {
				// The ordering is important for compatibility with MS and strange
				// Object.Equals () implementations
				if (linkSlots [cur].HashCode == hashCode && hcp.Equals (keySlots [cur], key))
					throw new ArgumentException ("An element with the same key already exists in the dictionary.");
				cur = linkSlots [cur].Next;
			}
			
			if (++count > threshold) {
				Resize ();
				index = (hashCode & int.MaxValue) % table.Length;
			}
			
			// find an empty slot
			cur = emptySlot;
			if (cur == NO_SLOT)
				cur = touchedSlots++;
			else 
				emptySlot = linkSlots [cur].Next;
			
			// store the hash code of the added item,
			// prepend the added item to its linked list,
			// update the hash table
			linkSlots [cur].HashCode = hashCode;
			linkSlots [cur].Next = table [index] - 1;
			table [index] = cur + 1;
			
			// store item's data 
			keySlots [cur] = key;
			valueSlots [cur] = value;
			
			generation++;
		}
		
		public IEqualityComparer<string> Comparer {
			get { return hcp; }
		}
		
		public void Clear ()
		{
			count = 0;
			// clear the hash table
			Array.Clear (table, 0, table.Length);
			// clear arrays
			Array.Clear (keySlots, 0, keySlots.Length);
			Array.Clear (valueSlots, 0, valueSlots.Length);
			Array.Clear (linkSlots, 0, linkSlots.Length);
			
			// empty the "empty slots chain"
			emptySlot = NO_SLOT;
			
			touchedSlots = 0;
			generation++;
		}
		
		public bool ContainsKey (string key)
		{
			if (key == null)
				return false;
			
			// get first item of linked list corresponding to given key
			int hashCode = hcp.GetHashCode (key) | HASH_FLAG;
			int cur = table [(hashCode & int.MaxValue) % table.Length] - 1;
			
			// walk linked list until right slot is found or end is reached
			while (cur != NO_SLOT) {
				// The ordering is important for compatibility with MS and strange
				// Object.Equals () implementations
				if (linkSlots [cur].HashCode == hashCode && hcp.Equals (keySlots [cur], key))
					return true;
				cur = linkSlots [cur].Next;
			}
			
			return false;
		}
		
		public bool ContainsValue (object value)
		{
			IEqualityComparer<object> cmp = EqualityComparer<object>.Default;
			
			for (int i = 0; i < table.Length; i++) {
				int cur = table [i] - 1;
				while (cur != NO_SLOT) {
					if (cmp.Equals (valueSlots [cur], value))
						return true;
					cur = linkSlots [cur].Next;
				}
			}
			return false;
		}
		
		[SecurityPermission (SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			
			info.AddValue ("Version", generation);
			info.AddValue ("Comparer", hcp);
			// MS.NET expects either *no* KeyValuePairs field (when count = 0)
			// or a non-null KeyValuePairs field. We don't omit the field to
			// remain compatible with older monos, but we also doesn't serialize
			// it as null to make MS.NET happy.
			KeyValuePair<string, object> [] data = new KeyValuePair<string,object> [count];
			if (count > 0)
				CopyTo (data, 0);
			info.AddValue ("HashSize", table.Length);
			info.AddValue ("KeyValuePairs", data);
		}
		
		public virtual void OnDeserialization (object sender)
		{
			if (serialization_info == null)
				return;
			
			int hashSize = 0;
			KeyValuePair<string, object> [] data = null;
			
			// We must use the enumerator because MS.NET doesn't
			// serialize "KeyValuePairs" for count = 0.
			SerializationInfoEnumerator e = serialization_info.GetEnumerator ();
			while (e.MoveNext ()) {
				switch (e.Name) {
				case "Version":
					generation = (int) e.Value;
					break;
					
				case "Comparer":
					hcp = (IEqualityComparer<string>) e.Value;
					break;
					
				case "HashSize":
					hashSize = (int) e.Value;
					break;
					
				case "KeyValuePairs":
					data = (KeyValuePair<string, object> []) e.Value;
					break;
				}
			}
			
			if (hcp == null)
				hcp = EqualityComparer<string>.Default;
			if (hashSize < INITIAL_SIZE)
				hashSize = INITIAL_SIZE;
			InitArrays (hashSize);
			count = 0;
			
			if (data != null) {
				for (int i = 0; i < data.Length; ++i)
					Add (data [i].Key, data [i].Value);
			}
			generation++;
			serialization_info = null;
		}
		
		public bool Remove (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			// get first item of linked list corresponding to given key
			int hashCode = hcp.GetHashCode (key) | HASH_FLAG;
			int index = (hashCode & int.MaxValue) % table.Length;
			int cur = table [index] - 1;
			
			// if there is no linked list, return false
			if (cur == NO_SLOT)
				return false;
			
			// walk linked list until right slot (and its predecessor) is
			// found or end is reached
			int prev = NO_SLOT;
			do {
				// The ordering is important for compatibility with MS and strange
				// Object.Equals () implementations
				if (linkSlots [cur].HashCode == hashCode && hcp.Equals (keySlots [cur], key))
					break;
				prev = cur;
				cur = linkSlots [cur].Next;
			} while (cur != NO_SLOT);
			
			// if we reached the end of the chain, return false
			if (cur == NO_SLOT)
				return false;
			
			count--;
			// remove slot from linked list
			// is slot at beginning of linked list?
			if (prev == NO_SLOT)
				table [index] = linkSlots [cur].Next + 1;
			else
				linkSlots [prev].Next = linkSlots [cur].Next;
			
			// mark slot as empty and prepend it to "empty slots chain"				
			linkSlots [cur].Next = emptySlot;
			emptySlot = cur;
			
			linkSlots [cur].HashCode = 0;
			// clear empty key and value slots
			keySlots [cur] = default (string);
			valueSlots [cur] = default (object);
			
			generation++;
			return true;
		}
		
		public bool TryGetValue (string key, out object value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			// get first item of linked list corresponding to given key
			int hashCode = hcp.GetHashCode (key) | HASH_FLAG;
			int cur = table [(hashCode & int.MaxValue) % table.Length] - 1;
			
			// walk linked list until right slot is found or end is reached
			while (cur != NO_SLOT) {
				// The ordering is important for compatibility with MS and strange
				// Object.Equals () implementations
				if (linkSlots [cur].HashCode == hashCode && hcp.Equals (keySlots [cur], key)) {
					value = valueSlots [cur];
					return true;
				}
				cur = linkSlots [cur].Next;
			}
			
			// we did not find the slot
			value = default (object);
			return false;
		}

		public bool hasOwnProperty(string key)
		{
			if (key == null) return false;
			object value;
			return TryGetValue(key, out value);
		}
		
		ICollection<string> IDictionary<string, object>.Keys {
			get { return Keys; }
		}
		
		ICollection<object> IDictionary<string, object>.Values {
			get { return Values; }
		}
		
#if NET_4_5
		IEnumerable<string> IReadOnlyDictionary<string, object>.Keys {
			get { return Keys; }
		}
		
		IEnumerable<object> IReadOnlyDictionary<string, object>.Values {
			get { return Values; }
		}
#endif
		
		public KeyCollection Keys {
			get { return new KeyCollection (this); }
		}
		
		public ValueCollection Values {
			get { return new ValueCollection (this); }
		}
		
		ICollection IDictionary.Keys {
			get { return Keys; }
		}
		
		ICollection IDictionary.Values {
			get { return Values; }
		}
		
		bool IDictionary.IsFixedSize {
			get { return false; }
		}
		
		bool IDictionary.IsReadOnly {
			get { return false; }
		}
		
		static string Tostring (object key)
		{
			// Optimize for the most common case - strings
			string keyString = key as string;
			if (keyString != null) {
				return keyString;
			}
			if (key == null)
				throw new ArgumentNullException ("key");

			return key.ToString();
		}
		
		static object Toobject (object value)
		{
			if (value == null && !typeof (object).IsValueType)
				return default (object);
			if (!(value is object))
				throw new ArgumentException ("not of type: " + typeof (object).ToString (), "value");
			return (object) value;
		}
		
		object IDictionary.this [object key] {
			get {
				return this [Tostring (key)];
			}
			set { this [Tostring (key)] = Toobject (value); }
		}
		
		void IDictionary.Add (object key, object value)
		{
			this.Add (Tostring (key), Toobject (value));
		}
		
		bool IDictionary.Contains (object key)
		{
			return ContainsKey(Tostring(key));
		}
		
		void IDictionary.Remove (object key)
		{
			Remove (Tostring(key));
		}
		
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		
		bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
			get { return false; }
		}
		
		void ICollection<KeyValuePair<string, object>>.Add (KeyValuePair<string, object> keyValuePair)
		{
			Add (keyValuePair.Key, keyValuePair.Value);
		}
		
		bool ICollection<KeyValuePair<string, object>>.Contains (KeyValuePair<string, object> keyValuePair)
		{
			return ContainsKeyValuePair (keyValuePair);
		}
		
		void ICollection<KeyValuePair<string, object>>.CopyTo (KeyValuePair<string, object> [] array, int index)
		{
			this.CopyTo (array, index);
		}
		
		bool ICollection<KeyValuePair<string, object>>.Remove (KeyValuePair<string, object> keyValuePair)
		{
			if (!ContainsKeyValuePair (keyValuePair))
				return false;
			
			return Remove (keyValuePair.Key);
		}
		
		bool ContainsKeyValuePair (KeyValuePair<string, object> pair)
		{
			object value;
			if (!TryGetValue (pair.Key, out value))
				return false;
			
			return EqualityComparer<object>.Default.Equals (pair.Value, value);
		}
		
		void ICollection.CopyTo (Array array, int index)
		{
			KeyValuePair<string, object> [] pairs = array as KeyValuePair<string, object> [];
			if (pairs != null) {
				this.CopyTo (pairs, index);
				return;
			}
			
			CopyToCheck (array, index);
			DictionaryEntry [] entries = array as DictionaryEntry [];
			if (entries != null) {
				for (int i = 0; i < touchedSlots; i++) {
					if ((linkSlots [i].HashCode & HASH_FLAG) != 0)
						entries [index++] = new DictionaryEntry (keySlots [i], valueSlots [i]);
				}
				return;
			}
			
			Do_ICollectionCopyTo<KeyValuePair<string, object>> (array, index, make_pair);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			// enumerate over values
			return Values.GetEnumerator();
		}

		IEnumerator IKeyEnumerable.GetKeyEnumerator ()
		{
			// enumerate over keys
			return Keys.GetEnumerator();
		}


		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator ()
		{
			return new Enumerator (this);
		}
		
		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new ShimEnumerator (this);
		}
		
		public Enumerator GetKVPEnumerator ()
		{
			return new Enumerator (this);
		}
		
		[Serializable]
		private class ShimEnumerator : IDictionaryEnumerator, IEnumerator
		{
			Enumerator host_enumerator;
			public ShimEnumerator (ExpandoObject host)
			{
				host_enumerator = host.GetKVPEnumerator ();
			}
			
			public void Dispose ()
			{
				host_enumerator.Dispose ();
			}
			
			public bool MoveNext ()
			{
				return host_enumerator.MoveNext ();
			}
			
			public DictionaryEntry Entry {
				get { return ((IDictionaryEnumerator) host_enumerator).Entry; }
			}
			
			public object Key {
				get { return host_enumerator.Current.Key; }
			}
			
			public object Value {
				get { return host_enumerator.Current.Value; }
			}
			
			// This is the raison d' etre of this $%!@$%@^@ class.
			// We want: IDictionary.GetEnumerator ().Current is DictionaryEntry
			public object Current {
				get { return Entry; }
			}
			
			public void Reset ()
			{
				host_enumerator.Reset ();
			}
		}
		
		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<string,object>>,
		IDisposable, IDictionaryEnumerator, IEnumerator
		{
			ExpandoObject dictionary;
			int next;
			int stamp;
			
			internal KeyValuePair<string, object> current;
			
			internal Enumerator (ExpandoObject dictionary)
			: this ()
			{
				this.dictionary = dictionary;
				stamp = dictionary.generation;
			}
			
			public bool MoveNext ()
			{
				VerifyState ();
				
				if (next < 0)
					return false;
				
				while (next < dictionary.touchedSlots) {
					int cur = next++;
					if ((dictionary.linkSlots [cur].HashCode & HASH_FLAG) != 0) {
						current = new KeyValuePair <string, object> (
							dictionary.keySlots [cur],
							dictionary.valueSlots [cur]
							);
						return true;
					}
				}
				
				next = -1;
				return false;
			}
			
			// No error checking happens.  Usually, Current is immediately preceded by a MoveNext(), so it's wasteful to check again
			public KeyValuePair<string, object> Current {
				get { return current; }
			}
			
			internal string CurrentKey {
				get {
					VerifyCurrent ();
					return current.Key;
				}
			}
			
			internal object CurrentValue {
				get {
					VerifyCurrent ();
					return current.Value;
				}
			}
			
			object IEnumerator.Current {
				get {
					VerifyCurrent ();
					return current;
				}
			}
			
			void IEnumerator.Reset ()
			{
				Reset ();
			}
			
			internal void Reset ()
			{
				VerifyState ();
				next = 0;
			}
			
			DictionaryEntry IDictionaryEnumerator.Entry {
				get {
					VerifyCurrent ();
					return new DictionaryEntry (current.Key, current.Value);
				}
			}
			
			object IDictionaryEnumerator.Key {
				get { return CurrentKey; }
			}
			
			object IDictionaryEnumerator.Value {
				get { return CurrentValue; }
			}
			
			void VerifyState ()
			{
				if (dictionary == null)
					throw new ObjectDisposedException (null);
				if (dictionary.generation != stamp)
					throw new InvalidOperationException ("Enumeration modified during iteration");
			}
			
			void VerifyCurrent ()
			{
				VerifyState ();
				if (next <= 0)
					throw new InvalidOperationException ("Current is not valid");
			}
			
			public void Dispose ()
			{
				dictionary = null;
			}
		}
		
		// This collection is a read only collection
		[Serializable]
		public sealed class KeyCollection : ICollection<string>, IEnumerable<string>, ICollection, IEnumerable {
			ExpandoObject dictionary;
			
			public KeyCollection (ExpandoObject dictionary)
			{
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");
				this.dictionary = dictionary;
			}
			
			
			public void CopyTo (string [] array, int index)
			{
				dictionary.CopyToCheck (array, index);
				dictionary.CopyKeys (array, index);
			}
			
			public Enumerator GetEnumerator ()
			{
				return new Enumerator (dictionary);
			}
			
			void ICollection<string>.Add (string item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
			
			void ICollection<string>.Clear ()
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
			
			bool ICollection<string>.Contains (string item)
			{
				return dictionary.ContainsKey (item);
			}
			
			bool ICollection<string>.Remove (string item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
			
			IEnumerator<string> IEnumerable<string>.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}
			
			void ICollection.CopyTo (Array array, int index)
			{
				var target = array as string [];
				if (target != null) {
					CopyTo (target, index);
					return;
				}
				
				dictionary.CopyToCheck (array, index);
				dictionary.Do_ICollectionCopyTo<string> (array, index, pick_key);
			}
			
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}
			
			public int Count {
				get { return dictionary.Count; }
			}
			
			bool ICollection<string>.IsReadOnly {
				get { return true; }
			}
			
			bool ICollection.IsSynchronized {
				get { return false; }
			}
			
			object ICollection.SyncRoot {
				get { return ((ICollection) dictionary).SyncRoot; }
			}
			
			[Serializable]
			public struct Enumerator : IEnumerator<string>, IDisposable, IEnumerator {
				ExpandoObject.Enumerator host_enumerator;
				
				internal Enumerator (ExpandoObject host)
				{
					host_enumerator = host.GetKVPEnumerator ();
				}
				
				public void Dispose ()
				{
					host_enumerator.Dispose ();
				}
				
				public bool MoveNext ()
				{
					return host_enumerator.MoveNext ();
				}
				
				public string Current {
					get { return host_enumerator.current.Key; }
				}
				
				object IEnumerator.Current {
					get { return host_enumerator.CurrentKey; }
				}
				
				void IEnumerator.Reset ()
				{
					host_enumerator.Reset ();
				}
			}
		}
		
		// This collection is a read only collection
		[Serializable]
		public sealed class ValueCollection : ICollection<object>, IEnumerable<object>, ICollection, IEnumerable {
			ExpandoObject dictionary;
			
			public ValueCollection (ExpandoObject dictionary)
			{
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");
				this.dictionary = dictionary;
			}
			
			public void CopyTo (object [] array, int index)
			{
				dictionary.CopyToCheck (array, index);
				dictionary.CopyValues (array, index);
			}
			
			public Enumerator GetEnumerator ()
			{
				return new Enumerator (dictionary);
			}
			
			void ICollection<object>.Add (object item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
			
			void ICollection<object>.Clear ()
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
			
			bool ICollection<object>.Contains (object item)
			{
				return dictionary.ContainsValue (item);
			}
			
			bool ICollection<object>.Remove (object item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
			
			IEnumerator<object> IEnumerable<object>.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}
			
			void ICollection.CopyTo (Array array, int index)
			{
				var target = array as object [];
				if (target != null) {
					CopyTo (target, index);
					return;
				}
				
				dictionary.CopyToCheck (array, index);
				dictionary.Do_ICollectionCopyTo<object> (array, index, pick_value);
			}
			
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}
			
			public int Count {
				get { return dictionary.Count; }
			}
			
			bool ICollection<object>.IsReadOnly {
				get { return true; }
			}
			
			bool ICollection.IsSynchronized {
				get { return false; }
			}
			
			object ICollection.SyncRoot {
				get { return ((ICollection) dictionary).SyncRoot; }
			}
			
			[Serializable]
			public struct Enumerator : IEnumerator<object>, IDisposable, IEnumerator {
				ExpandoObject.Enumerator host_enumerator;
				
				internal Enumerator (ExpandoObject host)
				{
					host_enumerator = host.GetKVPEnumerator ();
				}
				
				public void Dispose ()
				{
					host_enumerator.Dispose ();
				}
				
				public bool MoveNext ()
				{
					return host_enumerator.MoveNext ();
				}
				
				public object Current {
					get { return host_enumerator.current.Value; }
				}
				
				object IEnumerator.Current {
					get { return host_enumerator.CurrentValue; }
				}
				
				void IEnumerator.Reset ()
				{
					host_enumerator.Reset ();
				}
			}
		}

		[DebuggerDisplay("{value}", Name = "{key}")]
		internal class KeyValuePairDebugView
		{
			public object key;
			public object value;
			
			public KeyValuePairDebugView(object key, object value)
			{
				this.value = value;
				this.key = key;
			}
		}

		internal class ExpandoDebugView
		{
			private ExpandoObject expando;

			public ExpandoDebugView(ExpandoObject expando)
			{
				this.expando = expando;
			}
			
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public KeyValuePairDebugView[] Keys
			{
				get
				{
					var keys = new KeyValuePairDebugView[expando.Count];
					
					int i = 0;
					foreach(string key in expando.Keys)
					{
						keys[i] = new KeyValuePairDebugView(key, (object)expando[key]);
						i++;
					}
					return keys;
				}
			}
		}

		#region IDynamicClass implementation
		dynamic IDynamicClass.__GetDynamicValue(string name)
		{
			return this[name];
		}
		bool IDynamicClass.__TryGetDynamicValue(string name, out object value)
		{
			return this.TryGetValue(name, out value);
		}
		void IDynamicClass.__SetDynamicValue(string name, object value)
		{
			this[name] = value;
		}
		bool IDynamicClass.__DeleteDynamicValue(object name)
		{
			return this.Remove((string)name);
		}
		bool IDynamicClass.__HasDynamicValue(string name)
		{
			return this.ContainsKey(name);
		}
		IEnumerable IDynamicClass.__GetDynamicNames()
		{
			return this.Keys;
		}
		#endregion


		#region IFastDictionaryLookup<int> implementation
		int IFastDictionaryLookup<int>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			object result = this[key];
			return Convert<int>.FromObject(result);
		}

		void IFastDictionaryLookup<int>.SetValue(string key, int value, ref int hintStringID, ref int hintExpandoIndex)
		{
			this[key] = value;
		}
		#endregion

		#region IFastDictionaryLookup<uint> implementation
		uint IFastDictionaryLookup<uint>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			object result = this[key];
			return Convert<uint>.FromObject(result);
		}

		void IFastDictionaryLookup<uint>.SetValue(string key, uint value, ref int hintStringID, ref int hintExpandoIndex)
		{
			this[key] = value;
		}
		#endregion

		#region IFastDictionaryLookup<double> implementation
		double IFastDictionaryLookup<double>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			object result = this[key];
			return Convert<double>.FromObject(result);
		}

		void IFastDictionaryLookup<double>.SetValue(string key, double value, ref int hintStringID, ref int hintExpandoIndex)
		{
			this[key] = value;
		}
		#endregion

		#region IFastDictionaryLookup<bool> implementation
		bool IFastDictionaryLookup<bool>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			object result = this[key];
			return Convert<bool>.FromObject(result);
		}

		void IFastDictionaryLookup<bool>.SetValue(string key, bool value, ref int hintStringID, ref int hintExpandoIndex)
		{
			this[key] = value;
		}
		#endregion

		#region IFastDictionaryLookup<string> implementation
		string IFastDictionaryLookup<string>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			object result = this[key];
			return Convert<string>.FromObject(result);
		}

		void IFastDictionaryLookup<string>.SetValue(string key, string value, ref int hintStringID, ref int hintExpandoIndex)
		{
			this[key] = value;
		}
		#endregion

		#region IFastDictionaryLookup<object> implementation
		object IFastDictionaryLookup<object>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			return this[key];
		}

		void IFastDictionaryLookup<object>.SetValue(string key, object value, ref int hintStringID, ref int hintExpandoIndex)
		{
			this[key] = value;
		}
		#endregion
	}

#else

	/// <summary>
	/// Expando implementation that is used as an actionscript Object.
	/// 
	/// There are few things that are different from the default Expando implementation:
	///		- Keys are defined by String and IDs. This is designed in a way to make the lookup very fast if the information is cached.
	///		- Values are split in arrays to avoid boxing booleans, integers, doubles, etc... Objects, strings and other typesare still allocated on the heap.
	/// 		Note that even when they are stored as values, strings are coming from the StringPool.
	/// 	- To take advantage of StringIDs and caching, the expando is actually sorted by key IDs, and it is sorted only at the first lookup
	/// 		(so creation adds the element to the array quickly and we do only a single sort).
	/// 	- We are also measuring the memory usage of potential various implementations to figure out if one is much better than the others.
	/// 		Classes and strings are not measured as they would be the same for all implementations (assuming we use the string pool for each).
	/// </summary>
	[DebuggerDisplay("count = {Count}")]
//	[DebuggerTypeProxy(typeof(ArrayDebugView))]
	public class ExpandoObject : IDictionary<string, object>, IDictionary, /*ISerializable, IDeserializationCallback,*/ IDynamicClass, IKeyEnumerable,
		IFastDictionaryLookup<int>, IFastDictionaryLookup<uint>, IFastDictionaryLookup<double>, IFastDictionaryLookup<bool>, IFastDictionaryLookup<string>, IFastDictionaryLookup<object>
#if NET_4_5
		, IReadOnlyDictionary<string, object>
#endif
	{
		/// <summary>
		/// This is the old mode, everything is boxed.
		/// Calculation assumes that boxing a structure costs 8 bytes (might be actually more), aligned with 4 bytes.
		/// Then 16 bytes per KeyValuePair (Link, Key, Value) *1.10 for the load factor + 3*16 bytes for the three arrays.
		/// </summary>
		private static int	MemoryUsageModeA;

		/// <summary>
		/// This is a new mode, one array for booleans, one array for integers, one array for unsigned integers, one array for doubles,
		/// and one array for classes and strings.
		/// Note that this mode also has support for StringID, so the cost per key is 8 bytes (StringID, String pointer) per key.
		/// Then 16 bytes for the value type array + N bytes.
		/// And per value is per array type: 16 bytes + N * sizeof(T). The array is only allocated if there is at least one value of the type.
		/// There would be also an array for classes and strings.
		/// And we should add 10% to N for the load factor.
		/// </summary>
		private static int	MemoryUsageModeB;

		/// <summary>
		/// This is a new mode, one single array for the value types.
		/// Note that this mode also has support for StringID, so the cost per key is 8 bytes (StringID, String pointer) per key.
		/// Then 16 bytes for the value type array + N bytes.
		/// Then an array that contains doubles (16 bytes + N * sizeof(double)). Doubles can easily contain boolean, int, uint, int64, uint64, bool.
		/// Then a simlar array for classes and strings.
		/// And we should add 10% to N for the load factor.
		/// </summary>
		private static int	MemoryUsageModeC;

		// Note that by making sure StringID does not grow too much, we should be able to combine it with the value type (28 bits for StringIDs, 4 bits for value type).

		const uint			StringIDMask =	0x0fffffff;
		const uint			ValueTypeMask =		0xf0000000;
		const int			ValueTypeShift = 32 - 4;

		const int			ValueTypeObject =	0;
		const int			ValueTypeString =	1;
		const int			ValueTypeBool =		2;
		const int			ValueTypeInt =		3;
		const int			ValueTypeUint =		4;
		const int			ValueTypeDouble =	5;
		// Overtime we could also add short, ushort, Int64, Uint64, etc...
		// We are not using TypeCode as it does not match with 16 values (4 bits), and for some types, we actually do not have much usage (DateTime for example)

		[StructLayout(LayoutKind.Explicit)]
		[DebuggerDisplay("{DebugData}")]
		struct KeyValue
		{
			[FieldOffset(0)]
			public bool		ValueAsBool;
			[FieldOffset(0)]
			public int		ValueAsInt;
			[FieldOffset(0)]
			public uint		ValueAsUint;
			[FieldOffset(0)]
			public double	ValueAsDouble;
			[FieldOffset(8)]
			public object	ValueAsObject;			// This is probably going to work with Boehm GC, but what about SGen?
			[FieldOffset(8)]
			public string	ValueAsString;			// This is probably going to work with Boehm GC, but what about SGen?

			[FieldOffset(12)]
			public string	KeyAsString;
			[FieldOffset(16)]
			public uint		KeyAsStringID;			// Top 4 bits has the value type as well.

			public int StringID
			{
				get
				{
					return (int)(KeyAsStringID & StringIDMask);
				}
			}

			public int ValueType
			{
				get
				{
					return (int)(KeyAsStringID >> ValueTypeShift);
				}
			}

			public void SetKeyAndValueType(string key, int valueType)
			{
				int stringID;
				string pooledKey = StringPool.AddStringToPool(key, out stringID);
				KeyAsString = pooledKey;
				KeyAsStringID = (uint)(stringID | (valueType << ValueTypeShift));
			}

			public void SetKeyAndValueType(string pooledKey, int stringID, int valueType)
			{
				KeyAsString = pooledKey;
				KeyAsStringID = (uint)(stringID | (valueType << ValueTypeShift));
			}

			public void SetKey(string pooledKey, int stringID)
			{
				KeyAsString = pooledKey;
				KeyAsStringID &= ValueTypeMask;
				KeyAsStringID |= (uint)stringID;
			}

			public void SetValueType(int valueType)
			{
				KeyAsStringID &= StringIDMask;
				KeyAsStringID |= (uint)(valueType << ValueTypeShift);
			}

			public override string ToString ()
			{
				switch (ValueType)
				{
					case ValueTypeObject:
						return string.Format("{0} = object: {1}", KeyAsString, ValueAsObject);
					case ValueTypeString:
						return string.Format("{0} = string: {1}", KeyAsString, ValueAsString);
					case ValueTypeBool:
						return string.Format("{0} = bool:   {1}", KeyAsString, ValueAsBool);
					case ValueTypeInt:
						return string.Format("{0} = int:    {1}", KeyAsString, ValueAsInt);
					case ValueTypeUint:
						return string.Format("{0} = uint:   {1}", KeyAsString, ValueAsUint);
					case ValueTypeDouble:
						return string.Format("{0} = double: {1}", KeyAsString, ValueAsDouble);
					default:
						return string.Format("ValueType {0} not supported");
				}
			}

			internal string DebugData
			{
				get
				{
					return ToString();
				}
			}
		}

		/// <summary>
		/// When we allocate an expando, it has at minimum 4 key values.
		/// </summary>
		const int			MinimumN = 4;

		KeyValue[]			mKeyValues;
		int					mNumberOfKeyValues;

		// The number of changes made to this expando. Used by enumerators
		// to detect changes and invalidate themselves.
		private int generation;

		public int Generation { get { return generation; } }

		public ExpandoObject()
		{
			AddToMemoryUsage();
		}

		~ExpandoObject()
		{
			RemoveFromMemoryUsage();
		}

		private void CreateSpaceForKey(int index)
		{
			Stats.Increment(StatsCounter.Expando_AddKey);

			int lastIndex = mNumberOfKeyValues++;
			if (lastIndex == mKeyValues.Length)
			{
				Stats.Increment(StatsCounter.Expando_GrowExpando);

				// We have to grow the array as it is now too small.
				int newSize = lastIndex + (lastIndex >> 3) + 4;		// Increase size by 12.5% + 4
				// +1 for the one we are just going to fill, +3 for next values
				KeyValue[] newKeyValues = new KeyValue[newSize];

				// Copy the first half
				int firstHalfSize = index;
				Debug.Assert(firstHalfSize >= 0);
				if (firstHalfSize > 0)
				{
					Stats.Increment(StatsCounter.Expando_ArrayCopy);

					Array.Copy(mKeyValues, 0, newKeyValues, 0, firstHalfSize);
				}

				// Copy the second half, leaving the index position not copied
				int secondHalfSize = lastIndex - index;
				Debug.Assert(secondHalfSize >= 0);
				if (secondHalfSize > 0)
				{
					Stats.Increment(StatsCounter.Expando_ArrayCopy);

					Array.Copy(mKeyValues, index, newKeyValues, index + 1, secondHalfSize);
				}

				// Now that all data has been copied (with an empty space where index is, we can swap them
				mKeyValues = newKeyValues;
			}
			else
			{
				// Move the second half of the array
				int secondHalfSize = lastIndex - index;
				Debug.Assert(secondHalfSize >= 0);
				if (secondHalfSize > 0)
				{
					Stats.Increment(StatsCounter.Expando_ArrayCopy);

					Array.Copy(mKeyValues, index, mKeyValues, index + 1, secondHalfSize);
				}
			}
		}

		private int FindOrCreateSpaceForKey(string pooledKey, int stringID, out bool newKey)
		{
			// Let's make sure that the caller passed the right information
			Debug.Assert(StringPool.IsThisStringInPool(pooledKey, stringID));
			CheckValidity();

			if (mKeyValues != null)
			{
				int index = -1;
				bool found = FindLessOrEqualKey(pooledKey, false, ref stringID, ref index);
				if (found)
				{
					newKey = false;
					return index;
				}

				++index;	// Insert after the lesser ID
				CreateSpaceForKey(index);
				newKey = true;
				return index;
			}
			else
			{
				mKeyValues = new KeyValue[MinimumN];
				mNumberOfKeyValues = 1;
				newKey = true;
				return 0;
			}
		}

		private bool FindOrInsertKey(string key, ref int hintStringID, ref int hintIndex)
		{
			// In this case, the key might or might not be already in the pool

			if (mKeyValues != null)
			{
				bool found = FindLessOrEqualKey(key, false, ref hintStringID, ref hintIndex);
				if (found)
				{
					return false;
				}

				// Not found, we have to insert after the one before it
				++hintIndex;
				CreateSpaceForKey(hintIndex);
			}
			else
			{
				mKeyValues = new KeyValue[MinimumN];
				mNumberOfKeyValues = 1;
				hintIndex = 0;
			}
			// In this case, we want to make sure we insert the pooled string - We should be able to optimize this further (to not do a second lookup)
			string pooledKey = StringPool.AddStringToPool(key, out hintStringID);
			mKeyValues[hintIndex].SetKey(pooledKey, hintStringID);
			return true;
		}

		private int FindKey(string key)
		{
			int count = mNumberOfKeyValues;
			// At that point, we know that mKeyValues is allocated
			if (count <= 4)
			{
				Stats.Increment(StatsCounter.Expando_FastPathFind);

				// Linear search if few elements,
				// so we don't pay the stringID and binary search overhead (no insertion in this case)
				for (int i = 0 ; i < count ; ++i)
				{
					if (mKeyValues[i].KeyAsString == key)
					{
						return i;
					}
				}
			}
			else
			{
				int startIndex = 0;
				int endIndex = count - 1;
				int stringID = StringPool.GetStringID(key);
				if (stringID < 0)
				{
					// If it is not in the string pool, that means it is not in any expando
					return -1;
				}

				Stats.Increment(StatsCounter.Expando_SlowPathFind);

				while (startIndex <= endIndex)
				{
					int mid = (endIndex + startIndex) / 2;
					int midStringID = mKeyValues[mid].StringID;
					if (midStringID > stringID)
					{
						endIndex = mid - 1;
					}
					else if (midStringID < stringID)
					{
						startIndex = mid + 1;
					}
					else
					{
						// Found it, update the hint index too
						return mid;
					}
				}
			}
			return -1;
		}

		/// <summary>
		/// Finds the key or the closest smaller key.
		/// 
		/// For optimization reasons, we might want to have different implementations depending of the 
		/// value of exactOnly, or if we have / need hintIndex, hindStringID, etc...
		/// </summary>
		/// <returns><c>true</c>, if less or equal key was found, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		/// <param name="exactOnly">If set to <c>true</c> exact only.</param>
		/// <param name="hintStringID">Hint string I.</param>
		/// <param name="hintIndex">Hint index.</param>
		private bool FindLessOrEqualKey(string key, bool exactOnly, ref int hintStringID, ref int hintIndex)
		{
			if (mNumberOfKeyValues == 0)
			{
				return false;
			}

#if DEBUG
			if (hintIndex >= 0)
			{
				// If there is an index, it means that we had a string ID too.
				Debug.Assert(hintStringID >= 0);
			}
#endif

			int startIndex, endIndex;

			if ((hintIndex >= 0) && (hintIndex < mNumberOfKeyValues))
			{
				// We have a potentially valid hint index, let's see if it matches
				int stringIDAtIndex = mKeyValues[hintIndex].StringID;
				if (stringIDAtIndex == hintStringID)
				{
					// We have the correct index in 3 comparisons and 1 lookup
					Stats.Increment(StatsCounter.Expando_FastPathFind);
					return true;
				}

				// Index does not match, use the first lookup to initiate the binary seach
				if (stringIDAtIndex > hintStringID)
				{
					startIndex = 0;
					endIndex = hintIndex - 1;
					if (startIndex > endIndex)
					{
						// There can't be any match, lesser is before startIndex
						hintIndex = -1;
						Stats.Increment(StatsCounter.Expando_FastPathFind);
						return false;
					}
				}
				else
				{
					startIndex = hintIndex + 1;
					endIndex = mNumberOfKeyValues - 1;
					if (startIndex > endIndex)
					{
						// There can't be any match, lesser is the last index
						hintIndex = endIndex;
						Stats.Increment(StatsCounter.Expando_FastPathFind);
						return false;
					}
				}
			}
			else
			{
				// hintIndex is out of range, rescan everything
				startIndex = 0;
				endIndex = mNumberOfKeyValues - 1;
			}

			// Because we want to update the hintStringID, and doing the first lookup
			// we can directly do the binary search here.
			if (hintStringID < 0)
			{
				// The usage is going to be incremented here, even if the key arleady exists
				// Improve this so the usage counter does not get out of sync.
				key = StringPool.AddStringToPool(key, out hintStringID);
			}

			Stats.Increment(StatsCounter.Expando_SlowPathFind);

			int closestLesserIndex = -1;
			while (startIndex <= endIndex)
			{
				int midIndex = (endIndex + startIndex) / 2;
				int midStringID = mKeyValues[midIndex].StringID;
				if (midStringID > hintStringID)
				{
					endIndex = midIndex - 1;
				}
				else if (midStringID < hintStringID)
				{
					startIndex = midIndex + 1;
					closestLesserIndex = midIndex;
				}
				else
				{
					// Found it, update the hint index too
					hintIndex = midIndex;
					return true;
				}
			}

			if (exactOnly)
			{
				// We still keep the hintIndex, in case another object has the proper hintIndex
			}
			else
			{
				hintIndex = closestLesserIndex;
			}
			return false;
		}

		private string KeyToString(object key)
		{
			string keyAsString = key as string;
			if (keyAsString != null)
			{
				return keyAsString;
			}
			if (key == null)
			{
				// We do not allow null keys
				throw new ArgumentNullException();
			}
			return key.ToString();
		}

		public ICollection<string> Keys
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		public ICollection<object> Values
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		//
		// Indexer
		//
		public dynamic this [string key]
		{
			get
			{
				int index = FindKey(key);
				if (index >= 0)
				{
					return mKeyValues[index].ValueAsObject;
				}
				// We probably should return undefined instead of null
				return null;
			}
			set
			{
				bool newKey;
				int stringID;
				string pooledKey = StringPool.AddStringToPool(key, out stringID);
				int index = FindOrCreateSpaceForKey(pooledKey, stringID, out newKey);
				if (newKey)
				{
					mKeyValues[index].SetKeyAndValueType(pooledKey, stringID, ValueTypeObject);
				}
				else
				{
					// Not a new string, key string has already been pooled,
					// And with the current implementation, only object is being used, so type has already being set
					// We only have to set the new value, do not use StringPool yet for the value
				}
				mKeyValues[index].ValueAsObject = value;
				CheckValidity();
				++generation;
			}
		}

		public int Count
		{
			get
			{
				return mNumberOfKeyValues;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		//
		// Methods
		//
		public void Add (string key, object value)
		{
			throw new NotImplementedException();
		}
		public bool ContainsKey (string key)
		{
			if (key == null)
			{
				return false;
			}
			return FindKey(key) >= 0;
		}
		public bool Remove (string key)
		{
			// Remove is relatively rare, we can afford to have a slower code path by shifting the values
			int index = FindKey(key);
			if (index >= 0)
			{
				Stats.Increment(StatsCounter.Expando_Remove);
				Array.Copy(mKeyValues, index + 1, mKeyValues, index, mNumberOfKeyValues - index - 1);
				--mNumberOfKeyValues;
				CheckValidity();
				return true;
			}
			return false;
		}
		public bool TryGetValue (string key, out object value)
		{
			int index = FindKey(key);
			if (index >= 0)
			{
				Stats.Increment(StatsCounter.Expando_GetFromIDictionary);
				value = mKeyValues[index].ValueAsObject;
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		public void Add (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}
		public void Clear ()
		{
			throw new NotImplementedException();
		}
		public bool Contains (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo (KeyValuePair<string, object>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new ValueEnumerator(this);
		}

		IEnumerator IKeyEnumerable.GetKeyEnumerator()
		{
			return new KeyEnumerator(this);
		}

		#region IDictionary implementation

		void IDictionary.Add (object key, object value)
		{
			throw new NotImplementedException ();
		}

		bool IDictionary.Contains (object key)
		{
			throw new NotImplementedException ();
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		void IDictionary.Remove (object key)
		{
			throw new NotImplementedException ();
		}

		bool IDictionary.IsFixedSize {
			get {
				throw new NotImplementedException ();
			}
		}

		object IDictionary.this [object key] {
			get {
				Stats.Increment(StatsCounter.Expando_GetFromIDictionary);
				return this[KeyToString(key)];
			}
			set {
				Stats.Increment(StatsCounter.Expando_GetFromIDictionary);
				this[(string)key] = value;
			}
		}

		ICollection IDictionary.Keys {
			get {
				return new KeyCollection(this);
			}
		}

		ICollection IDictionary.Values {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region ICollection implementation

		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		bool ICollection.IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		object ICollection.SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		class KeyEnumerator : IEnumerator
		{
			private ExpandoObject mExpando;
			private int mIndex;
			private int mGeneration;

			public KeyEnumerator(ExpandoObject expando)
			{
				mExpando = expando;
				Reset();
			}

			public bool MoveNext ()
			{
				mIndex++;
				return mIndex < mExpando.Count;							// Continue as long as we did not reach the end
			}

			public void Reset ()
			{
				mIndex = -1;
				mGeneration = mExpando.Generation;
			}

			public object Current {
				get {
					if (mGeneration != mExpando.Generation)
					{
						throw new InvalidOperationException();			// Collection has been modified during enumeration
					}
					return mExpando.mKeyValues[mIndex].KeyAsString;
				}
			}
		}

		class ValueEnumerator : IEnumerator
		{
			private ExpandoObject mExpando;
			private int mIndex;
			private int mGeneration;

			public ValueEnumerator(ExpandoObject expando)
			{
				mExpando = expando;
				Reset();
			}

			public bool MoveNext ()
			{
				mIndex++;
				return mIndex < mExpando.Count;							// Continue as long as we did not reach the end
			}

			public void Reset ()
			{
				mIndex = -1;
				mGeneration = mExpando.Generation;
			}

			public object Current {
				get {
					if (mGeneration != mExpando.Generation)
					{
						throw new InvalidOperationException();			// Collection has been modified during enumeration
					}
					switch (mExpando.mKeyValues[mIndex].ValueType)
					{
						case ValueTypeObject:
							return mExpando.mKeyValues[mIndex].ValueAsObject;
						case ValueTypeString:
							return mExpando.mKeyValues[mIndex].ValueAsString;
						case ValueTypeBool:
							Stats.Increment(StatsCounter.Expando_Boxing);
							return mExpando.mKeyValues[mIndex].ValueAsBool;
						case ValueTypeInt:
							Stats.Increment(StatsCounter.Expando_Boxing);
							return mExpando.mKeyValues[mIndex].ValueAsInt;
						case ValueTypeUint:
							Stats.Increment(StatsCounter.Expando_Boxing);
							return mExpando.mKeyValues[mIndex].ValueAsUint;
						case ValueTypeDouble:
							Stats.Increment(StatsCounter.Expando_Boxing);
							return mExpando.mKeyValues[mIndex].ValueAsDouble;
						default:
							throw new NotImplementedException();
					}
				}
			}
		}

		class KeyCollection : ICollection
		{
			ExpandoObject mExpandoObject;
			public KeyCollection(ExpandoObject expandoObject)
			{
				mExpandoObject = expandoObject;
			}

			public void CopyTo (Array array, int index)
			{
				throw new NotImplementedException ();
			}

			public int Count {
				get {
					return mExpandoObject.Count;
				}
			}

			public bool IsSynchronized {
				get {
					throw new NotImplementedException ();
				}
			}

			public object SyncRoot {
				get {
					throw new NotImplementedException ();
				}
			}

			public IEnumerator GetEnumerator ()
			{
				return new KeyEnumerator(mExpandoObject);
			}
		}

		#region IDynamicClass implementation
		dynamic IDynamicClass.__GetDynamicValue(string name)
		{
			return this[name];
		}
		bool IDynamicClass.__TryGetDynamicValue(string name, out object value)
		{
			return this.TryGetValue(name, out value);
		}
		void IDynamicClass.__SetDynamicValue(string name, object value)
		{
			this[name] = value;
		}
		bool IDynamicClass.__DeleteDynamicValue(object name)
		{
			return this.Remove((string)name);
		}
		bool IDynamicClass.__HasDynamicValue(string name)
		{
			return this.ContainsKey(name);
		}
		IEnumerable IDynamicClass.__GetDynamicNames()
		{
			return this.Keys;
		}
		#endregion

		int ConvertValueToInt(int index)
		{
			switch (mKeyValues[index].ValueType)
			{
			case ValueTypeInt:
					return mKeyValues[index].ValueAsInt;
			case ValueTypeUint:
					return (int)mKeyValues[index].ValueAsUint;
			case ValueTypeDouble:
					return (int)mKeyValues[index].ValueAsDouble;
			case ValueTypeBool:
					return mKeyValues[index].ValueAsBool ? 1 : 0;
			case ValueTypeString:
					// We probably should convert here...
					Stats.Increment(StatsCounter.Expando_ParseFromString);
					return int.Parse(mKeyValues[index].ValueAsString);
			case ValueTypeObject:
					Stats.Increment(StatsCounter.Expando_UnboxingAndConvert);
					return Dynamic.ConvertValue<int>(mKeyValues[index].ValueAsObject);
			default:
				throw new NotSupportedException();
			}
		}

		void SetValueToInt(int index, int value)
		{
			mKeyValues[index].SetValueType(ValueTypeInt);
			mKeyValues[index].ValueAsInt = value;
		}

		uint ConvertValueToUint(int index)
		{
			switch (mKeyValues[index].ValueType)
			{
			case ValueTypeInt:
					return (uint)mKeyValues[index].ValueAsInt;
			case ValueTypeUint:
					return mKeyValues[index].ValueAsUint;
			case ValueTypeDouble:
					return (uint)mKeyValues[index].ValueAsDouble;
			case ValueTypeBool:
					return mKeyValues[index].ValueAsBool ? 1u : 0u;
			case ValueTypeString:
					// We probably should convert here...
					Stats.Increment(StatsCounter.Expando_ParseFromString);
					return uint.Parse(mKeyValues[index].ValueAsString);
			case ValueTypeObject:
					Stats.Increment(StatsCounter.Expando_UnboxingAndConvert);
					return Dynamic.ConvertValue<uint>(mKeyValues[index].ValueAsObject);
			default:
				throw new NotSupportedException();
			}
		}

		void SetValueToUint(int index, uint value)
		{
			mKeyValues[index].SetValueType(ValueTypeUint);
			mKeyValues[index].ValueAsUint = value;
		}

		double ConvertValueToDouble(int index)
		{
			switch (mKeyValues[index].ValueType)
			{
			case ValueTypeInt:
					return (double)mKeyValues[index].ValueAsInt;
			case ValueTypeUint:
					return (double)mKeyValues[index].ValueAsUint;
			case ValueTypeDouble:
					return mKeyValues[index].ValueAsDouble;
			case ValueTypeBool:
					return mKeyValues[index].ValueAsBool ? 1.0 : 0.0;
			case ValueTypeString:
					// We probably should convert here...
					Stats.Increment(StatsCounter.Expando_ParseFromString);
					return double.Parse(mKeyValues[index].ValueAsString);
			case ValueTypeObject:
					Stats.Increment(StatsCounter.Expando_UnboxingAndConvert);
					return Dynamic.ConvertValue<double>(mKeyValues[index].ValueAsObject);
			default:
				throw new NotSupportedException();
			}
		}

		void SetValueToDouble(int index, double value)
		{
			mKeyValues[index].SetValueType(ValueTypeDouble);
			mKeyValues[index].ValueAsDouble = value;
		}

		bool ConvertValueToBool(int index)
		{
			switch (mKeyValues[index].ValueType)
			{
			case ValueTypeInt:
					return (mKeyValues[index].ValueAsInt != 0);
			case ValueTypeUint:
					return (mKeyValues[index].ValueAsUint != 0);
			case ValueTypeDouble:
					return (mKeyValues[index].ValueAsDouble != 0.0);
			case ValueTypeBool:
					return mKeyValues[index].ValueAsBool;
			case ValueTypeString:
					// We probably should convert here...
					Stats.Increment(StatsCounter.Expando_ParseFromString);
					return bool.Parse(mKeyValues[index].ValueAsString);
			case ValueTypeObject:
					Stats.Increment(StatsCounter.Expando_UnboxingAndConvert);
					return Dynamic.ConvertValue<bool>(mKeyValues[index].ValueAsObject);
			default:
				throw new NotSupportedException();
			}
		}

		void SetValueToBool(int index, bool value)
		{
			mKeyValues[index].SetValueType(ValueTypeBool);
			mKeyValues[index].ValueAsBool = value;
		}

		string ConvertValueToString(int index)
		{
			switch (mKeyValues[index].ValueType)
			{
				case ValueTypeInt:
					return mKeyValues[index].ValueAsInt.ToString();
				case ValueTypeUint:
					return mKeyValues[index].ValueAsUint.ToString();
				case ValueTypeDouble:
					return mKeyValues[index].ValueAsDouble.ToString();
				case ValueTypeBool:
					return mKeyValues[index].ValueAsBool.ToString();
				case ValueTypeString:
					// We probably should convert here...
					return mKeyValues[index].ValueAsString;
				case ValueTypeObject:
					Stats.Increment(StatsCounter.Expando_ObjectToString);
					return Dynamic.ConvertValue<string>(mKeyValues[index].ValueAsObject);
				default:
					throw new NotSupportedException();
			}
		}

		void SetValueToString(int index, string value)
		{
			mKeyValues[index].SetValueType(ValueTypeString);
			mKeyValues[index].ValueAsString = value;
		}

		object ConvertValueToObject(int index)
		{
			switch (mKeyValues[index].ValueType)
			{
			case ValueTypeInt:
					Stats.Increment(StatsCounter.Expando_Boxing);
					return mKeyValues[index].ValueAsInt;
			case ValueTypeUint:
					Stats.Increment(StatsCounter.Expando_Boxing);
					return mKeyValues[index].ValueAsUint;
			case ValueTypeDouble:
					Stats.Increment(StatsCounter.Expando_Boxing);
					return mKeyValues[index].ValueAsDouble;
			case ValueTypeBool:
					Stats.Increment(StatsCounter.Expando_Boxing);
					return mKeyValues[index].ValueAsBool;
			case ValueTypeString:
					// We probably should convert here...
					return mKeyValues[index].ValueAsString;
			case ValueTypeObject:
					return mKeyValues[index].ValueAsObject;
			default:
				throw new NotSupportedException();
			}
		}

		void SetValueToObject(int index, object value)
		{
			mKeyValues[index].SetValueType(ValueTypeObject);
			mKeyValues[index].ValueAsObject = value;
			CheckValidity();
		}

		#region IFastDictionaryLookup<int> implementation
		int IFastDictionaryLookup<int>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			CheckValidity();
			if (FindLessOrEqualKey(key, true, ref hintStringID, ref hintExpandoIndex) == false)
			{
				return 0;
			}
			return ConvertValueToInt(hintExpandoIndex);
		}

		void IFastDictionaryLookup<int>.SetValue(string key, int value, ref int hintStringID, ref int hintExpandoIndex)
		{
			FindOrInsertKey(key, ref hintStringID, ref hintExpandoIndex);
			SetValueToInt(hintExpandoIndex, value);
			CheckValidity();
		}
		#endregion

		#region IFastDictionaryLookup<uint> implementation
		uint IFastDictionaryLookup<uint>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			CheckValidity();
			if (FindLessOrEqualKey(key, true, ref hintStringID, ref hintExpandoIndex) == false)
			{
				return 0;
			}
			return ConvertValueToUint(hintExpandoIndex);
		}

		void IFastDictionaryLookup<uint>.SetValue(string key, uint value, ref int hintStringID, ref int hintExpandoIndex)
		{
			FindOrInsertKey(key, ref hintStringID, ref hintExpandoIndex);
			SetValueToUint(hintExpandoIndex, value);
			CheckValidity();
		}
		#endregion

		#region IFastDictionaryLookup<double> implementation
		double IFastDictionaryLookup<double>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			CheckValidity();
			if (FindLessOrEqualKey(key, true, ref hintStringID, ref hintExpandoIndex) == false)
			{
				return 0.0;
			}
			return ConvertValueToDouble(hintExpandoIndex);
		}

		void IFastDictionaryLookup<double>.SetValue(string key, double value, ref int hintStringID, ref int hintExpandoIndex)
		{
			FindOrInsertKey(key, ref hintStringID, ref hintExpandoIndex);
			SetValueToDouble(hintExpandoIndex, value);
			CheckValidity();
		}
		#endregion

		#region IFastDictionaryLookup<bool> implementation
		bool IFastDictionaryLookup<bool>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			CheckValidity();
			if (FindLessOrEqualKey(key, true, ref hintStringID, ref hintExpandoIndex) == false)
			{
				return false;
			}
			return ConvertValueToBool(hintExpandoIndex);
		}

		void IFastDictionaryLookup<bool>.SetValue(string key, bool value, ref int hintStringID, ref int hintExpandoIndex)
		{
			FindOrInsertKey(key, ref hintStringID, ref hintExpandoIndex);
			SetValueToBool(hintExpandoIndex, value);
			CheckValidity();
		}
		#endregion

		#region IFastDictionaryLookup<string> implementation
		string IFastDictionaryLookup<string>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			CheckValidity();
			if (FindLessOrEqualKey(key, true, ref hintStringID, ref hintExpandoIndex) == false)
			{
				return null;
			}
			return ConvertValueToString(hintExpandoIndex);
		}

		void IFastDictionaryLookup<string>.SetValue(string key, string value, ref int hintStringID, ref int hintExpandoIndex)
		{
			FindOrInsertKey(key, ref hintStringID, ref hintExpandoIndex);
			SetValueToString(hintExpandoIndex, value);
			CheckValidity();
		}
		#endregion

		#region IFastDictionaryLookup<object> implementation
		object IFastDictionaryLookup<object>.GetValue(string key, ref int hintStringID, ref int hintExpandoIndex)
		{
			CheckValidity();
			if (FindLessOrEqualKey(key, true, ref hintStringID, ref hintExpandoIndex) == false)
			{
				return null;		// We probably should return undefined here
			}
			return ConvertValueToObject(hintExpandoIndex);
		}

		void IFastDictionaryLookup<object>.SetValue(string key, object value, ref int hintStringID, ref int hintExpandoIndex)
		{
			FindOrInsertKey(key, ref hintStringID, ref hintExpandoIndex);
			string valueAsString = value as string;
			if (valueAsString != null)
			{
				SetValueToString(hintExpandoIndex, valueAsString);
			}
			else
			{
				SetValueToObject(hintExpandoIndex, value);
			}
			CheckValidity();
		}
		#endregion

		[Conditional("DEBUG")]
		void CheckValidity()
		{
			// Make sure that the transformations are correct
			int previousStringID = -1;
			for (int i = 0 ; i < mNumberOfKeyValues ; ++i)
			{
				int stringID = mKeyValues[i].StringID;
				if (StringPool.IsThisStringInPool(mKeyValues[i].KeyAsString, stringID) == false)
				{
					// Only pooled string should be stored for keys
					throw new InvalidOperationException();
				}

				if (stringID <= previousStringID)
				{
					throw new InvalidOperationException();
				}
				previousStringID = stringID;
			}
		}


		[Conditional("DEBUG")]
		void AddToMemoryUsage()
		{
			MemoryUsageModeA += GetMemoryUsageModeA();
			MemoryUsageModeB += GetMemoryUsageModeB();
			MemoryUsageModeC += GetMemoryUsageModeC();
		}

		[Conditional("DEBUG")]
		void RemoveFromMemoryUsage()
		{
			MemoryUsageModeA -= GetMemoryUsageModeA();
			MemoryUsageModeB -= GetMemoryUsageModeB();
			MemoryUsageModeC -= GetMemoryUsageModeC();
		}

		int GetMemoryUsageModeA()
		{
			return 0;
		}

		int GetMemoryUsageModeB()
		{
			return 0;
		}

		int GetMemoryUsageModeC()
		{
			return 0;
		}
	}



#endif
}

