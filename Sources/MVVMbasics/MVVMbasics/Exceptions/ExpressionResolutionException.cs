/*
 * (c) 2015-2018 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Exceptions
{
	/// <summary>
	/// Simple exception thrown by ServiceLocator when trying to retrieve a service that has not been registered.
	/// </summary>
	public class ExpressionResolutionException : Exception
	{
		public ExpressionResolutionException()
			: base()
		{
		}

		public ExpressionResolutionException(string message)
			: base(message)
		{
		}
	}
}
