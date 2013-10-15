using System;

namespace PlayScript.Tooling
{
	public class DefaultValueAttribute : Attribute
	{
		public object DefaultValue;

		public DefaultValueAttribute (object value)
		{
		}
	}
}

