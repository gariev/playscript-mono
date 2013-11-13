using System;
using System.Collections.Generic;


namespace PlayScript.Tooling
{
	public class BlockDef
	{
		internal Schema _schema;

		private bool _frozen;
		private string _name;
		private uint _crcId;
		private List<FieldDef> _fields = new List<FieldDef>();
		private Dictionary<string,FieldDef> _fieldsByName = new Dictionary<string,FieldDef>();
		private Dictionary<uint,FieldDef> _fieldsByCrcId = new Dictionary<uint,FieldDef>();
		private int _length;
		private bool _hasSparseFields;
		private FieldDef _nameField;

		public BlockDef ()
		{
		}

		private void CheckFrozen() {
			if (_frozen) 
				throw new InvalidOperationException ("Data is read only.");
		}

		public void Freeze()
		{
			_frozen = true;
		}

		public string Name {
			get { return _name; }
			set {
				CheckFrozen ();
				_name = value; 
				_crcId = Crc32.Calculate (_name);
			}
		}

		public uint CrcId {
			get { return _crcId; }
		}

		public int Length {
			get {
				return _length;
			}
			set {
				CheckFrozen ();
				_length = value;
			}
		}

		public bool HasSparseFields {
			get { 
				return _hasSparseFields; 
			}
			set {
				CheckFrozen ();
				_hasSparseFields = value;
			}
		}

		public FieldDef NameField {
			get { 
				return _nameField;
			}
		}

		public int NumFields {
			get { 
				return _fields.Count;
			}
		}

		public FieldDef GetFieldAt(int index) 
		{
			return _fields [index];
		}

		public int GetFieldIndex(string fieldName)
		{
			for (var i = 0; i < _fields.Count; i++) {
				if (_fields [i].Name == fieldName) 
					return i;
			}
			return -1;
		}

		public FieldDef GetFieldByName(string fieldName)
		{
			return _fieldsByName [fieldName];
		}

		public FieldDef GetFieldByCrcId(uint crcId)
		{
			if (crcId == 0)
				return null;
			return _fieldsByCrcId [crcId];
		}

		public void AddField(FieldDef fieldInfo) 
		{
			CheckFrozen ();
			fieldInfo._parent = this;
			fieldInfo._schema = this._schema;
			_fields.Add (fieldInfo);
			_fieldsByName.Add (fieldInfo.Name, fieldInfo);
			_fieldsByCrcId.Add (fieldInfo.CrcId, fieldInfo);
			if (fieldInfo.IsSparse)
				_hasSparseFields = true;
			if (fieldInfo.IsName) {
				if (_nameField != null) 
					throw new InvalidOperationException ("Only one name field may be added to a block.");
				_nameField = fieldInfo;
			}
			fieldInfo.Freeze ();
		}

		public IntPtr AllocBlock()
		{
			return AssetPool.AllocBuffer (_length);
		}

		public IntPtr CloneBlock(IntPtr block)
		{
			void* oldPtr = block.ToPointer ();
			if (oldPtr == (void*)0)
				return new IntPtr (0);

			IntPtr newBlock = AllocBlock ();
			void* newPtr = newBlock.ToPointer ();

			uint* s = (uint*)oldPtr;
			uint* d = (uint*)newPtr;

			int i;
			int len = _length >> 2;
			for (i = 0; i < len; i++) {
				*d++ = *s++;
			}

			len = _fields.Count;
			for (i = 0; i < len; i++) {
				var field = _fields [i];
				if (field.FieldType == FieldType.Block) {
					*((void**)((byte*)newPtr + field.Start)) = CloneBlock (new IntPtr (*(void**)((byte*)oldPtr + field.Start))).ToPointer ();
				} else if (field.FieldType == FieldType.String) {
					*((void**)((byte*)newPtr + field.Start)) = AssetPool.CloneString (new IntPtr (*(void**)((byte*)oldPtr + field.Start))).ToPointer ();
				} else if (field.FieldType == FieldType.Array) {

				}
			}
		}

		public void CalulateStructureLayout()
		{
			int len = _fields.Count;
		}

		/// <summary>
		/// Fixup all pointers in the contiguous block buffer pointed to by 'block' to be relative to the start of block.
		/// </summary>
		/// <param name="block">The beginning of the contiguous block data buffer.</param>
		public void FixupPointers(IntPtr block)
		{
		}

		/// <summary>
		/// Unfixup all pointers in the contiguous block buffer pointed to by 'block' to be relative to 0.
		/// </summary>
		/// <param name="block">The beginning of the contiguous block data buffer.</param>
		public void UnfixupPointers(IntPtr block)
		{
		}

	}
}

