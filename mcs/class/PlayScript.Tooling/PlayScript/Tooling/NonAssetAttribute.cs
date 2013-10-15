using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Marks an element (field, property, class) in an asset graph as not belonging to the graph.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class NonAssetAttribute : Attribute
	{
		public NonAssetAttribute ()
		{
		}
	}
}

