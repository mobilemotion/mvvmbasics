/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;
using System.Windows;
using Microsoft.Phone.Shell;
using MVVMbasics.Helpers;

namespace MVVMbasics
{
    /// <summary>
    /// Base Application class the phone application can be derived from.
    /// </summary>
    public class BaseApplication : Application
	{
		#region Members

		/// <summary>
		/// Flag that indicates whether the application was awakened from hidden (not tombstoned) state or not.
		/// (Only stored until the next <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> event,
		/// cleared afterwards).
		/// </summary>
		internal bool ReturnedFromHiddenState = false;

		/// <summary>
		/// Flag that indicates whether the application was awakened from tombstoned state or not. (Only stored until
		/// the next <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> event, cleared
		/// afterwards).
		/// </summary>
		internal bool ReturnedFromTombstonedState = false;

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
		/// Constructor. Registers a PhoneApplicationService event to the application that contains implementations of
		/// the Launching, Activated, Deactivated, and Closing events.
		/// </summary>
		public BaseApplication()
		{
			PhoneApplicationService phoneApplicationService = new PhoneApplicationService();
			phoneApplicationService.Launching += OnLaunching;
			phoneApplicationService.Activated += OnActivated;
			phoneApplicationService.Deactivated += OnDeactivated;
			phoneApplicationService.Closing += OnClosing;
			ApplicationLifetimeObjects.Add(phoneApplicationService);
		}

		#endregion

		#region Event handler methods

		/// <summary>
		/// Method that is executed when the application is launching (eg, from Start), but not when the application is
		/// reactivated.
		/// Subclasses can override this method to run custom code when the application is launching.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void OnLaunching(object sender, LaunchingEventArgs e)
		{
		}

		/// <summary>
		/// Method that is executed when the application is activated (brought to foreground), but not when the
		/// application is first launched. If the application is reactivated from tombstoning, sets a flag to the
		/// currently active view to indicate that ViewState must be set to Awakaned.
		/// Subclasses can override this method to run custom code when the application is activated, but should not
		/// forget to add a call to base.OnActivated(sender, e).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
	    public virtual void OnActivated(object sender, ActivatedEventArgs e)
		{
			if (!e.IsApplicationInstancePreserved)
			{
			    // The application was tombstoned: ensure that correct ViewState is passed to in Viewmodel's
				// OnNavigatedTo event
				ReturnedFromTombstonedState = true;
			}
			else
			{
				// The application was hidden: ensure that correct ViewState is passed to in Viewmodel's
				// OnNavigatedTo event
				ReturnedFromHiddenState = true;
			}
	    }

		/// <summary>
		/// Method that is executed when the application is deactivated (sent to background), but not when the
		/// application is closing.
		/// Subclasses can override this method to run custom code when the application is deactivated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void OnDeactivated(object sender, DeactivatedEventArgs e)
		{
		}

		/// <summary>
		/// Method that is executed when the application is closing (eg, user hit Back), but not when the application is
		/// deactivated.
		/// Subclasses can override this method to run custom code when the application is closing.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void OnClosing(object sender, ClosingEventArgs e)
		{
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
