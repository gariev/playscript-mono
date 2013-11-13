using System;

namespace PlayScript.Tooling
{
	public static unsafe class EditorHelpers
	{
		public static void AssignObjectData(void* target, object obj) {
			if (obj == null) {
				*(void**)target = null;
			} else {
				var editable = obj as IEditable;
				if (editable != null) {
					*(void**)target = editable.Data.ToPointer;
				} else {
					var s = obj as string;
					if (s != null) {
						*(void**)target = AssetPool.AllocString (s);
					} else {
						throw new Exception ("Invalid object type.");
					}
				}
			}
		}
	}
}

