/*
 * (c) 2013-2018 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Exceptions
{
	/// <summary>
	/// Simple exception thrown by ServiceLocator when trying to retrieve a service that has not been registered.
	/// </summary>
	public class ServiceNotFoundException : Exception
	{
		public ServiceNotFoundException()
			: base()
		{
		}

		public ServiceNotFoundException(string message)
			: base(message)
		{
		}
	}
}
