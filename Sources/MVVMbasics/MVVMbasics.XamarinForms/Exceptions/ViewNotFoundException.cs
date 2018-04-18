/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;

namespace MVVMbasics.Exceptions
{
	/// <summary>
	/// Simple exception thrown by Navigator service when trying to retrieve a view that has not been registered.
	/// </summary>
	public class ViewNotFoundException : Exception
	{
		public ViewNotFoundException()
			: base()
		{
		}

		public ViewNotFoundException(string message)
			: base(message)
		{
		}
	}
}