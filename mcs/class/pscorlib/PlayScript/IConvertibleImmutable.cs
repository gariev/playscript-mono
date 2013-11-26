using System;

namespace PlayScript 
{
	// Implemented by immutable objects which can be converted to mutable
	public interface IConvertibleImmutable 
	{
		// Is the object currently immutable
		bool IsImmutable { get; }
		// Converts the object to mutable
		void ConvertToMutable();
		// The immutable parent which must also be converted to mutable if this child
		// is converted to mutable
		IConvertibleImmutable ConvertibleImmutableParent { get; }
	}

}