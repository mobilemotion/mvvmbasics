/*
 * (c) 2014-2015 Andreas Kuntner
 */

using System;
using Xamarin.Forms;

namespace MVVMbasics.Helpers
{
	/// <summary>
	/// Helper class that provides functions for invoking actions on the UI thread.
	/// </summary>
	internal class DispatcherHelper : IDispatcherHelper
	{
		/// <summary>
		/// Method that instructs the dispatcher to invoke an action.
		/// </summary>
		/// <param name="action">Action to be invoked by the dispatcher</param>
		public void RunOnMainThread(Action action)
		{
			Device.BeginInvokeOnMainThread (action);
		}

		/// <summary>
		/// Method that checks whether the program logic currently runs on the UI thread.
		/// </summary>
		/// <returns>TRUE if the dispatcher has access to the UI thread, FALSE otherwise</returns>
		public bool IsRunningOnMainThread()
		{
			// Unfortunately, Xamarin.Forms currently doesn't provide any functionality for checking this, so always return false
			return false;
		}
	}
}
