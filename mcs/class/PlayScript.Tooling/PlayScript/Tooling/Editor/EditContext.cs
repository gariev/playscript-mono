using System;
using System.Collections.Generic;

namespace PlayScript.Tooling
{
	/// <summary>
	/// An undo record that can be added to the current edit context's undo redo stack.
	/// </summary>
	public class UndoRecord
	{
		/// <summary>
		/// The name of the undo.
		/// </summary>
		public string Name;
		/// <summary>
		/// The undo action.
		/// </summary>
		public Action Undo;
		/// <summary>
		/// The redo action.
		/// </summary>
		public Action Redo;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlayScript.Tooling.UndoRecord"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="undo">Undo.</param>
		/// <param name="redo">Redo.</param>
		public UndoRecord(string name, Action undo, Action redo) {
			name = name;
			Undo = undo;
			Redo = redo;
		}
	}

	/// <summary>
	/// Group undo record.
	/// </summary>
	public class GroupUndoRecord : UndoRecord
	{
		/// <summary>
		/// The original starting index in the top level undo this record was created from.
		/// </summary>
		public int StartUndoPos;
		/// <summary>
		/// The child undo records contained in this group.
		/// </summary>
		public List<UndoRecord> ChildUndos;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlayScript.Tooling.GroupUndoRecord"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="startUndoPos">Start undo position.</param>
		public GroupUndoRecord(string name, int startUndoPos) : base(name, null, null) {
			StartUndoPos = startUndoPos;
		}
	}

	/// <summary>
	/// Class which describes a single context where one or more related assets are being editing.
	/// </summary>
	public class EditContext
	{
		private List<UndoRecord> _undoStack = new List<UndoRecord> ();
		private int _undoPos = 0;

		private List<GroupUndoRecord> _undoGroups = new List<GroupUndoRecord> ();

		private List<IEditable> _selected = new List<IEditable> ();

		private bool _editMode = true;
		private bool _recordUndo = true;

		public EditContext ()
		{
		}

		/// <summary>
		/// Undoes the previous change.
		/// </summary>
		public void Undo() 
		{
			if (_undoPos > 0) {
				_undoStack [_undoPos - 1].Undo ();
				_undoPos--;
			}
		}

		/// <summary>
		/// Redoes the next change.
		/// </summary>
		public void Redo()
		{
			if (_undoPos < _undoStack.Count) {
				_undoStack [_undoPos].Redo ();
				_undoPos++;
			}
		}

		/// <summary>
		/// Adds a new undo record to the undo stack.
		/// </summary>
		/// <param name="undoRec">Undo rec.</param>
		public void AddUndo(UndoRecord undoRec) {
			if (_recordUndo) {
				for (var i = _undoStack.Count - 1; i >= _undoPos; i--) 
					_undoStack.RemoveAt (i);
				_undoStack.Add (undoRec);
			}
		}

		/// <summary>
		/// Starts an undo group which collapses a set of undo redo's into a single record.
		/// </summary>
		/// <param name="undoRec">The name of the undo group.</param>
		public GroupUndoRecord StartUndoGroup(string name) {
			GroupUndoRecord undoGroup = new GroupUndoRecord (name, _undoPos);
			return undoGroup;
		}

		/// <summary>
		/// Ends an undo group which collapses a set of undo redo's into a single record.
		/// </summary>
		public void EndUndoGroup(GroupUndoRecord undoGroup) {
			if (_recordUndo && undoGroup.StartUndoPos < _undoPos) {
				List<UndoRecord> undoRecs = new List<UndoRecord> ();
				for (var i = undoGroup.StartUndoPos; i < _undoPos; i++) {
					undoRecs.Add (_undoStack [i]);
				}
				for (var j = _undoPos - 1; j >= undoGroup.StartUndoPos; j--) {
					undoRecs.RemoveAt (j);
				}
				undoGroup.ChildUndos = undoRecs;
				undoGroup.Undo = () => {
					for (var k = undoRecs.Count - 1; k >= 0; k--)
						undoRecs [k].Undo ();
				};
				undoGroup.Redo = () => {
					for (var l = 0; l < undoRecs.Count; l++)
						undoRecs [l].Redo ();
				};
				AddUndo (undoGroup);
			}
		}

		/// <summary>
		/// Clears the current undo/redo stack.
		/// </summary>
		public void ClearUndo() {
			_undoStack.Clear ();
			_undoPos = 0;
		}

		/// <summary>
		/// Gets and sets the list of selected objects.
		/// </summary>
		/// <value>The selected objects.</value>
		public IEnumerable<IEditable> Selected {
			get {
				return _selected;
			}
			set {
			}
		}

		/// <summary>
		/// The current timeline used to create animation keys.  If null, no animation can be created.
		/// </summary>
		/// <value>The timeline.</value>
		public ITimeline Timeline {
			get {
				return null;
			}
			set {
			}
		}

		/// <summary>
		/// Turns on/off editing mode.  
		/// </summary>
		/// <remarks>
		/// When in editing mode, changes to asset objects affect both their 'current' and 'initial' value.  When editing mode is off
		/// changes to asset objects only affect their 'current' value, and their 'initial' value is unchanged.  Also, no undo-redo
		/// records are created when editing mode is off.
		/// </remarks>
		/// <value><c>true</c> if edit mode; otherwise, <c>false</c>.</value>
		public bool EditMode {
			get {
				return _editMode;
			}
			set {
				_editMode = value;
			}
		}

		/// <summary>
		/// True if undo/redo information should be tracked automatically when objects are modified.
		/// </summary>
		/// <value><c>true</c> if record undo; otherwise, <c>false</c>.</value>
		public bool RecordUndo {
			get {
				return _recordUndo;
			}
			set {
				_recordUndo = true;
			}
		}

		/// <summary>
		/// True if modifying values will automatically create or modify animation keys in the current timeline.
		/// </summary>
		/// <value><c>true</c> if record animation; otherwise, <c>false</c>.</value>
		public bool RecordAnim {
			get {
				return false;
			}
			set {
			}
		}

		/// <summary>
		/// Gets an object in the context given it's unique id.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="uid">Uid.</param>
		IEditable GetObject (uint uid) {
			return null;
		}

		/// <summary>
		/// Gets a value from an object, logging changing, undo/redo and sending change events to any listening ui.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index.</param>
		public void GetValue(IEditable obj, int fieldId, ref Value value, int index = -1)
		{
			return obj.GetValue (fieldId, ref value, index);
		}

		/// <summary>
		/// Sets a value for an object, logging changing, undo/redo and sending change events to any lisening ui.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="fieldId">Field identifier.</param>
		/// <param name="value">Value.</param>
		/// <param name="index">Index.</param>
		public void SetValue(IEditable obj, int fieldId, ref Value value, int index = -1)
		{
			Value oldValue = new Value();
			if (RecordUndo) {
				oldValue = obj.GetValue (fieldId, ref value, index);
				AddUndo( new UndoRecord("Set Value", 
				                        () => { obj.SetValue(fieldId, oldValue, index); }, 
										() => { obj.SetValue(fieldId, value, index); }));
				RecordUndo = false;
				bool saveEditMode = EditMode;
				EditMode = false;
				try {
					obj.SetValue (fieldId, value, index);
				} finally {
					RecordUndo = true;
					EditMode = saveEditMode;
				}
			} else {
				obj.SetValue (fieldId, value, index);
			}
			
		}

	}
}

