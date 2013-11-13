using System;
using System.Collections.Generic;

namespace PlayScript.Tooling
{
	public class Schema
	{
		// Topmost schema in the application - includes all definitions.
		private static Schema _topSchema;

		private bool _frozen;
		private string _name;
		private List<BlockDef> _blocks;
		private Dictionary<string, BlockDef> _blocksByName = new Dictionary<string, BlockDef>();
		private Dictionary<uint, BlockDef> _blocksByCrcId = new Dictionary<uint, BlockDef>();

		static Schema()
		{
			_topSchema = new Schema ();
			_topSchema.Name = "top";
			_topSchema.Freeze ();
		}

		public Schema ()
		{
		}

		/// <summary>
		/// Topmost schema in the application which includes all definitions.
		/// </summary>
		/// <value>The top schema.</value>
		public static Schema TopSchema {
			get {
				return _topSchema;
			}
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
			}
		}

		public int NumBlocks {
			get { 
				return _blocks.Count;
			}
		}

		public BlockDef GetBlockAt(int index) 
		{
			return _blocks [index];
		}

		public int GetBlockIndex(string blockName)
		{
			for (var i = 0; i < _blocks.Count; i++) {
				if (_blocks [i].Name == blockName) 
					return i;
			}
			return -1;
		}

		public BlockDef GetBlockByName(string blockName)
		{
			if (blockName == null)
				return null;
			return _blocksByName [blockName];
		}

		public BlockDef GetBlockByCrcId(uint crcId)
		{
			if (crcId == 0)
				return null;
			return _blocksByCrcId [crcId];
		}

		public void AddBlock(BlockDef block) 
		{
			CheckFrozen ();
			block._schema = this;
			for (var i = 0; i < block.NumFields; i++) {
				FieldDef field = block.GetFieldAt (i);
				field._schema = this;
			}
			_blocks.Add (block);
			_blocksByName.Add (block.Name, block);
			_blocksByCrcId.Add (block.CrcId, block);
			block.Freeze ();
		}

		public void ResolveReferences()
		{
			foreach (var block in _blocks) {
				int numFields = block.NumFields;
				for (var i = 0; i < numFields; i++) {
					var field = block.GetFieldAt (i);
					if (field.BlockDefName != null) {
						field._blockDef = _blocksByName [field.BlockDefName];
						if (field._blockDef == null) {
							throw InvalidOperationException ("Unable to resolve reference to '" + field.BlockDefName + "' block.");
						}
					}
				}
			}
		}

		public void Merge(Schema schema)
		{
			foreach (var block in schema._blocks) {
				AddBlock (block);
			}
			ResolveReferences ();
		}

	}
}

