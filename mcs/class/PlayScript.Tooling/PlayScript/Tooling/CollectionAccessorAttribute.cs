using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// The type of collection accessor method.
	/// </summary>
	public enum CollectionAccessorType {
		/// <summary>Method which adds an element to the collection.</summary>
		Add,
		/// <summary>Method which removes an element from the collection.</summary>
		Remove,
		/// <summary>Method which gets an element from the collection.</summary>
		Get,
		/// <summary>Method which sets an element in the collection.</summary>
		Set,
		/// <summary>Method which inserts an element into the collection.</summary>
		Insert,
		/// <summary>Method which returns the index of an element in the collection.</summary>
		IndexOf,
		/// <summary>Method which returns true if an element is contained in the collection.</summary>
		Contains,
		/// <summary>Method which is used to initialize a new collection with a vector of initial items.</summary>
		Initialize
	}

	/// <summary>
	/// Marks a collection accessor method for a collection with a given name.
	/// </summary>
	/// <remarks>
	/// This attribute is used to mark the set of methods which modify a private internal collection.  The 
	/// tooling system abstracts these methods into a single virtual collection field which is treated as a normal
	/// array or dictionary.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CollectionAccessorAttribute : Attribute
	{
		/// <summary>The name of the collection field.</summary>
		public string Name;
		/// <summary>The accessor method type.</summary>
		public CollectionAccessorType AccessorType;

		public CollectionAccessorAttribute ()
		{
		}
	}
}

