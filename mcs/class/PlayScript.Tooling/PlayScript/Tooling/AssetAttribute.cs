using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Marks a field, property, or .
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class AssetAttribute : Attribute
	{
		public AssetAttribute ()
		{
		}
	}
}

