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
		/// Gets the field info array for the fields in this asset.
		/// </summary>
		/// <returns>The field info.</returns>
		FieldInfo[] GetFieldInfo();

		/// <summary>
		/// Gets the integer id for the given field.
		/// </summary>
		/// <returns>The integer id for the field.</returns>
		/// <param name="fieldName">Field name.</param>
		int GetIdForField (string fieldName);

		/// <summary>
		/// Gets a value, optionally from a collection.
		/// </summary>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index in collection, or -1 if not used.</param>
		void GetValue(int fieldId, ref Value value, int index = -1);

		/// <summary>
		/// Sets a value, optionally from a collection.
		/// </summary>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index in collection, or -1 if not used.</param>
		void SetValue(int fieldId, ref Value value, int index = -1);

		/// <summary>
		/// Adds a value to a collection.
		/// </summary>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		void AddValue(int fieldId, ref Value value);

		/// <summary>
		/// Inserts a value into a collection.
		/// </summary>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index.</param>
		void InsertValue(int fieldId, ref Value value, int index = -1);

		/// <summary>
		/// Removes a value from a collection.
		/// </summary>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index.</param>
		void RemoveValue(int fieldId, ref Value value, int index = -1);

		/// <summary>
		/// Moves an element in a collection.
		/// </summary>
		/// <param name="OldIndex">oldIndex.</param>
		/// <param name="NewIndex">newIndex.</param>
		void MoveElement(int fieldId, int oldIndex, int newIndex);

		/// <summary>
		/// Implements a depth first visitor for all of the children of this object.
		/// </summary>
		/// <param name="">Visitor method which returns false to short circuit the visiting.</param>
		void Visit(Func<IEditable,bool> visitor);
	}
}

