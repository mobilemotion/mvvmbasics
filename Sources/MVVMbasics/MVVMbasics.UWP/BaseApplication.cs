/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MVVMbasics.Helpers;
using MVVMbasics.Views;

namespace MVVMbasics
{
	/// <summary>
	/// Base Application class the tablet application can be derived from.
	/// </summary>
	public class BaseApplication : Application
	{
		#region Members

		/// <summary>
		/// Reference to the currently active View (to be used within the <see cref="OnSuspending">OnSuspending</see>
		/// and <see cref="OnResuming">OnResuming</see> event handlers to fire the corresponding events of the current
		/// View.
		/// </summary>
		internal BaseView CurrentView;

		/// <summary>
		/// Reference to the current View's main frame (to be used for navigation by the 
		/// <see cref="NavigatorService">NavigatorService</see>).
		/// </summary>
		public Frame RootFrame
		{
			get
			{
				if (_rootFrame == null)
				{
					// Assign the App's root frame, if not done already
					var frame = Window.Current?.Content as Frame;
					if (frame == null)
					{
						frame = new Frame();
						frame.CacheSize = 1;
						if (Window.Current != null)
							Window.Current.Content = frame;
					}
					_rootFrame = frame;
				}
				return _rootFrame;
			}
			internal set { _rootFrame = value; }
		}
		private Frame _rootFrame;

		/// <summary>
		/// Simple IoC container that can be used to register MVVM services that are used within Viewmodels.
		/// </summary>
		public ServiceRegistry Services
		{
			get { return _services; }
		}
		private readonly ServiceRegistry _services = new ServiceRegistry();

		//TODO: remove deprecated
		/// <summary>
		/// <see cref="MVVMbasics.Helpers.ServiceLocator">ServiceLocator</see> instance that can be used throughout the
		/// application. Services should be registered to it in the BaseApplication's constructor. This ServiceLocator
		/// instance will be passed on to all Viewmodels.
		/// </summary>
		[Obsolete(@"Use App.Services or any 3rd party IoC container instead")]
		public ServiceLocator ServiceLocator
		{
			get { return _serviceLocator; }
		}
		private readonly ServiceLocator _serviceLocator = new ServiceLocator();

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor. Registers the OnSuspending and OnResuming event handlers.
		/// </summary>
		public BaseApplication()
		{
			this.Suspending += OnSuspending;
			this.Resuming += OnResuming;
		}

		#endregion

		#region Event handler methods

		/// <summary>
		/// Handles the Applications's Suspending event by calling the current page's OnSuspending method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			if (CurrentView != null)
			{
				CurrentView.OnSuspending();
			}
		}

		/// <summary>
		/// Handles the Applications's Resuming event by calling the current page's OnResuming method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnResuming(object sender, object e)
		{
			if (CurrentView != null)
			{
				CurrentView.OnResuming();
			}
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Method that resolves service references. Override this method to use MVVMbasics with any IoC container.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		protected internal virtual object Resolve(Type type)
		{
			if (Services != null)
				return Services.Resolve(type);
			else
				return Activator.CreateInstance(type);
		}
		#endregion
	}
}
