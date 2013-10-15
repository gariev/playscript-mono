using System;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Marks a field as being sparse (only non-default values are stored).
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class SparseAttribute : Attribute
	{
		public SparseAttribute ()
		{
		}
	}
}

