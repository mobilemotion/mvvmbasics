/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;
using System.Windows;
using MVVMbasics.Helpers;
using MVVMbasics.Services;
using MVVMbasics.Viewmodels;

namespace MVVMbasics
{
    /// <summary>
    /// Base Application class the desktop application can be derived from.
    /// </summary>
    public class BaseApplication : Application
    {
		/// <summary>
		/// Reference to the Viewmodel that is registered to the currently active View. This is needed by
		/// <see cref="NavigatorService">NavigatorService</see> in order to call
		/// the OnNavigatedFrom event when navigating to a new View.
		/// </summary>
	    internal BaseViewmodel CurrentViewmodel;

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
    }
}
