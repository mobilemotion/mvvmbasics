/*
 * (c) 2014-2015 Andreas Kuntner
 */

using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MVVMbasics.Helpers
{
	/// <summary>
	/// Helper class that provides functions for invoking actions on the UI thread.
	/// </summary>
	internal class DispatcherHelper : IDispatcherHelper
	{
		private readonly CoreDispatcher _dispatcher;

		/// <summary>
		/// Constructor. Retrieves the currently active view's dispatcher, or fails with an exception.
		/// </summary>
		internal DispatcherHelper()
		{
			_dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
		}

		/// <summary>
		/// Method that instructs the dispatcher to invoke an action.
		/// </summary>
		/// <param name="action">Action to be invoked by the dispatcher</param>
		public void RunOnMainThread(Action action)
		{
#pragma warning disable 4014
			_dispatcher.RunAsync(CoreDispatcherPriority.Normal, action.Invoke);
#pragma warning restore 4014
		}

		/// <summary>
		/// Method that checks whether the program logic currently runs on the UI thread.
		/// </summary>
		/// <returns>TRUE if the dispatcher has access to the UI thread, FALSE otherwise</returns>
		public bool IsRunningOnMainThread()
		{
			return _dispatcher.HasThreadAccess;
		}
	}
}
