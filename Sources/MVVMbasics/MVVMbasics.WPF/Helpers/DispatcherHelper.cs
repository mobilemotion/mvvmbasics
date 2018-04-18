/*
 * (c) 2014-2018 Andreas Kuntner
 */

using System;
using System.Windows;
using System.Windows.Threading;

namespace MVVMbasics.Helpers
{
	/// <summary>
	/// Helper class that provides functions for invoking actions on the UI thread.
	/// </summary>
	internal class DispatcherHelper : IDispatcherHelper
	{
		private readonly Dispatcher _dispatcher;

		/// <summary>
		/// Constructor. Retrieves the currently active view's dispatcher, or fails with an exception.
		/// </summary>
		internal DispatcherHelper()
		{
			_dispatcher = Application.Current.Dispatcher;
		}

		/// <summary>
		/// Method that instructs the dispatcher to invoke an action.
		/// </summary>
		/// <param name="action">Action to be invoked by the dispatcher</param>
		public void RunOnMainThread(Action action)
		{
			_dispatcher.BeginInvoke(action);
		}

		/// <summary>
		/// Method that checks whether the program logic currently runs on the UI thread.
		/// </summary>
		/// <returns>TRUE if the dispatcher has access to the UI thread, FALSE otherwise</returns>
		public bool IsRunningOnMainThread()
		{
			return _dispatcher.CheckAccess();
		}
	}
}
