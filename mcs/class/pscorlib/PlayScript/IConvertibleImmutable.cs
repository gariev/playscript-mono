using System;

namespace PlayScript 
{
	public interface IConvertibleImmutable 
	{
		bool IsImmutable { get; }
		void ConvertToMutable();
		IConvertibleImmutable ConvertibleImmutableParent { get; }
	}

}