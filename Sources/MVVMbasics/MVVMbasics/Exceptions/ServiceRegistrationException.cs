/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;

namespace MVVMbasics.Exceptions
{
	/// <summary>
	/// Simple exception thrown by ServiceLocator when trying to register a service that has already been registered.
	/// </summary>
	public class ServiceRegistrationException : Exception
	{
		public ServiceRegistrationException()
			: base()
		{
		}

        public ServiceRegistrationException(string message)
			: base(message)
		{
		}
	}
}