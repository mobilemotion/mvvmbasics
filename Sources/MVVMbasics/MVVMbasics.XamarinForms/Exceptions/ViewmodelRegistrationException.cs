/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;

namespace MVVMbasics.Exceptions
{
	/// <summary>
	/// Simple exception thrown by Navigator service when trying to register a viewmodel that has already been
	/// registered.
	/// </summary>
	public class ViewmodelRegistrationException : Exception
	{
		public ViewmodelRegistrationException()
			: base()
		{
		}

		public ViewmodelRegistrationException(string message)
			: base(message)
		{
		}
	}
}