using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Interface automatically implemented by compiler for all objects included in an asset graph when -tooling:editor is specified.
	/// </summary>
	public interface IEditable : IAssetObject
	{
		/// <summary>
		/// Unique id used to identify this object (should be a monotonically increasing integer).
		/// </summary>
		/// <value>The uid.</value>
		uint Uid { get; }

		/// <summary>
		/// Gets the asset pool this asset object's data is stored in.
		/// </summary>
		/// <value>The asset pool.</value>
		AssetPool AssetPool { get; }

		/// <summary>
		/// Gets the edit context - or null if the object is not being edited.
		/// </summary>
		/// <value>The edit context.</value>
		EditContext EditContext { get; }

		/// <summary>
		/// Gets the asset parent for this object - or null if this is the asset root object.
		/// </summary>
		/// <value>The asset parent.</value>
		IEditable AssetParent { get; }

		/// <summary>
		/// Gets the asset root of this object - returns 'this' if this is the root object.
		/// </summary>
		/// <value>The asset root.</value>
		IEditable AssetRoot { get; }

		/// <summary>
		/// True if this object is currently selected.
		/// </summary>
		/// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
		bool IsSelected { get; set; }

		/// <summary>
		/// Gets the block definition describing the binary backing data for this asset.
		/// </summary>
		/// <returns>The block definition.</returns>
		BlockDef BlockDef { get; }

		/// <summary>
		/// Gets the integer index for a given field.
		/// </summary>
		/// <returns>The index of the field found or -1 if field was not found.</returns>
		/// <param name="fieldName">The field name.</param>
		int GetFieldIndex(string fieldName);

		/// <summary>
		/// Gets a value, optionally from a collection.
		/// </summary>
		/// <param name="fieldIdx">Field index.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index in collection, or -1 if not used.</param>
		void GetValue(int fieldIdx, ref Value value, int index = -1);

		/// <summary>
		/// Sets a value, optionally from a collection.
		/// </summary>
		/// <param name="fieldIdx">Field index.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index in collection, or -1 if not used.</param>
		void SetValue(int fieldIdx, ref Value value, int index = -1);

		/// <summary>
		/// Adds a value to a collection.
		/// </summary>
		/// <param name="fieldIdx">Field index.</param>
		/// <param name="value">Value.</param>
		void AddValue(int fieldIdx, ref Value value);

		/// <summary>
		/// Inserts a value into a collection.
		/// </summary>
		/// <param name="fieldIdx">Field index.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index.</param>
		void InsertValue(int fieldId, ref Value value, int index = -1);

		/// <summary>
		/// Removes a value from a collection.
		/// </summary>
		/// <param name="fieldIdx">Field index.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index.</param>
		void RemoveValue(int fieldIdx, ref Value value, int index = -1);

		/// <summary>
		/// Moves an element in a collection.
		/// </summary>
		/// <param name="fieldIdx">The field index.</param>
		/// <param name="oldIndex">The old element index.</param>
		/// <param name="newIndex">The new element index.</param>
		void MoveElement(int fieldIdx, int oldIndex, int newIndex);

		/// <summary>
		/// Implements a depth first visitor for all of the children of this object.
		/// </summary>
		/// <param name="">Visitor method which returns false to short circuit the traversal.</param>
		void Visit(Func<IEditable,bool> visitor);
	}
}

