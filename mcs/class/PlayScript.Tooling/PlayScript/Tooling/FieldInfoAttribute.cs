using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Field metadata information.
	/// </summary>
	public class FieldInfoAttribute : Attribute
	{
		public string Name;
		public string Category;
		public string Help;
		public object DefaultValue;
	}
}

