/*
 * (c) 2013-2018 Andreas Kuntner
 */

using MVVMbasics.Views;
using Xamarin.Forms;
using MVVMbasics.Helpers;
using MVVMbasics.Services;
using System;

namespace MVVMbasics
{
	/// <summary>
	/// Base Application class the tablet application can be derived from.
	/// </summary>
	public class BaseApplication : Application
	{
		#region Members

		/// <summary>
		/// Reference to the currently active View (to be used within the <see cref="OnSleep">OnSleep</see> and
		/// <see cref="OnResume">OnResume</see> event handlers to fire the corresponding events of the current View.
		/// </summary>
		internal BaseView CurrentView;

		/// <summary>
		/// Flag that indicates whether the application is currently suspended. Needed to avoid duplicate suspending
		/// and resuming events.
		/// </summary>
		private bool _isHibernated = false;

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

		#region Public methods

		/// <summary>
		/// Defines the application's main page. If the provided page is not of type <code>NavigationPage</code> and
		/// a <see cref="MVVMbasics.Services.NavigatorService">NavigatorService</see> with more than one
		/// View-to-Viewmodel mapping is registered, it is automatically wrapped within a <code>NavigationPage</code>.
		/// </summary>
		/// <typeparam name="T">Type of the application's main page.</typeparam>
		protected void SetStartupPage<T>() where T : Page
		{
			var page = Activator.CreateInstance<T> ();
			if (page != null)
				SetStartupPage (page);
		}

		/// <summary>
		/// Defines the application's main page. If the provided page is not of type <code>NavigationPage</code> and
		/// a <see cref="MVVMbasics.Services.NavigatorService">NavigatorService</see> with more than one
		/// View-to-Viewmodel mapping is registered, it is automatically wrapped within a <code>NavigationPage</code>.
		/// </summary>
		/// <param name="page">The application's main page.</param>
		protected void SetStartupPage(Page page)
		{
			// If the given startup page is of type NavigationPage, just register it as MainPage. Otherwise...
			if (!(page is NavigationPage))
			{
				// ...check whether a NavigatorService is registered somewhere in the application
				NavigatorService navigatorService = null;

				try
				{
#pragma warning disable 618
					if (ServiceLocator != null && ServiceLocator.Contains<INavigatorService>())
						navigatorService = ServiceLocator.Retrieve<INavigatorService>() as NavigatorService;
					else
						navigatorService = Resolve(typeof(INavigatorService)) as NavigatorService;
#pragma warning restore 618
				}
				catch (Exception)
				{
					navigatorService = null;
				}

				// ...and check whether more than one View/Viewmodel mapping has been registered to this
				// NavigatorService
				if (navigatorService != null)
				{
					if (navigatorService.Mappings.Count > 1)
					{
						// If so, wrap the given page within a NavigationPage and set this as MainPage
						MainPage = new NavigationPage (page);
						return;
					}
				}
			}

			// If no NavigatorService has been found, or only one View/Viewmodel mapping is present, it's not
			// necessary to wrap the given page within a NavigationPage, so just register it as MainPage
			MainPage = page;
		}

		/// <summary>
		/// Method that instantiates Viewmodels and resolves service references. Override this method to use
		/// MVVMbasics with any IoC container.
		/// </summary>
		/// <param name="type">Type of the class to be resolved and instantiated.</param>
		/// <returns>Instance of the desired type.</returns>
		protected internal virtual object Resolve(Type type)
		{
			if (Services != null)
				return Services.Resolve(type);
			else
				return Activator.CreateInstance(type);
		}

		#endregion

		#region Event handler methods

		/// <summary>
		/// Handles the Applications's Suspending event by calling the current page's OnSuspending method.
		/// </summary>
		protected override void OnSleep()
		{
			if (CurrentView != null)
			{
				CurrentView.OnSuspending();
			}
			_isHibernated = true;
		}

		/// <summary>
		/// Handles the Applications's Resuming event by calling the current page's OnResuming method.
		/// </summary>
		protected override void OnResume()
		{
			if (CurrentView != null && _isHibernated)
			{
				CurrentView.OnResuming();
			}
			_isHibernated = false;
		}

		#endregion
	}
}
