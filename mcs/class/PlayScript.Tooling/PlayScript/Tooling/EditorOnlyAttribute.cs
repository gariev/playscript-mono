using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Marks a field as only existing when compiled in EDITOR mode.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class EditorOnlyAttribute : Attribute
	{
		public EditorOnlyAttribute ()
		{
		}
	}
}

