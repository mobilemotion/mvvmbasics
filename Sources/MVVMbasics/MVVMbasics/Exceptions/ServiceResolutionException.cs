/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;

namespace MVVMbasics.Exceptions
{
	/// <summary>
	/// Simple exception thrown by ServiceRegistry when resolving a service failed.
	/// </summary>
	public class ServiceResolutionException : Exception
	{
		public ServiceResolutionException()
			: base()
		{
		}

		public ServiceResolutionException(string message)
			: base(message)
		{
		}
	}
}