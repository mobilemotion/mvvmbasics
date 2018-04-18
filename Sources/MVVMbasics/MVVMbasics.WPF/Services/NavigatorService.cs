/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using MVVMbasics.Attributes;
using MVVMbasics.Exceptions;
using MVVMbasics.Viewmodels;
using MVVMbasics.Views;

namespace MVVMbasics.Services
{
	/// <summary>
	/// Service which provides page navigation functionality. Includes both "standard" navigation methods which navigate
	/// backwards or to a specified URI, as well as a View-/Viewmodel-Locator that stores mappings of View to Viewmodel
	/// and allows to directly navigate to a View that is associated with a given Viewmodel.
	/// </summary>
	[MvvmService]
	public class NavigatorService : INavigatorService
	{
		#region Members

		/// <summary>
		/// Stores all mapping of Viewmodels (keys in the <c>Dictionary</c>) to Views (windows, stored as values in the
		/// <c>Dictionary</c>). Both for Viewmodels and Views, only their respective types are stored instead of actual
		/// instances.
		/// </summary>
		private readonly Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();

		/// <summary>
		/// Collection of all windows that are currently open. The last object in this collection is the currently
		/// active window. Necessary for keeping a call history to allow backwards navigation and passing back
		/// parameters to the previously active window.
		/// </summary>
		private readonly List<Window> _windowStack = new List<Window>();

		/// <summary>
		/// List of parameters to be passed back to the previously active window, whenever the current window is closed.
		/// </summary>
		private ParameterList _backParameters = new ParameterList();

		#endregion

		#region Register Methods

		/// <summary>
		/// Registers a mapping of Viewmodel to View. Both must be supplied as <c>Type</c> object. The Viewmodel type
		/// must be a subtype of <see cref="MVVMbasics.Viewmodels.BaseViewmodel">BaseViewmodel</see>. The View type must
		/// be a subtype of <see cref="BaseView">BaseView</see>.
		/// Exceptions will be thrown if one of these type constraints is not fulfilled, or if a mapping with the
		/// specified Viewmodel type exists already.
		/// </summary>
		public void Register<T>(object view) where T : BaseViewmodel
		{
			if (!(view is Type))
				throw new ViewmodelRegistrationException("View to be registered must be supplied as type 'Type'.");
			if (!(((Type)view).IsSubclassOf(typeof(BaseView))))
				throw new ViewmodelRegistrationException("Viewmodel may only be registered to View that extends 'BaseView'.");
			if (_mappings.ContainsKey(typeof(T)))
				throw new ViewmodelRegistrationException("Viewmodel of type '" + typeof(T).FullName +
														 "' is already registered to a View.");
			Type window = (Type)view;
			_mappings.Add(typeof(T), window);
		}

		/// <summary>
		/// Traverses all Views that are located within a given namespace and registers all that provide a
		/// <see cref="MVVMbasics.Attributes.MvvmNavigationTargetAttribute">MvvmNavigationTarget</see> attribute.
		/// An exception is thrown if one of the traversed Views references a Viewmodel type that has been registered
		/// already.
		/// </summary>
		/// <param name="assembly">Assembly to be scanned. If NULL, the calling assembly will be scanned.</param>
		public void RegisterAll(Assembly assembly)
		{
			RegisterAll(null, assembly);
		}

		/// <summary>
		/// Traverses all Views that are located within a given namespace inside a given assembly and registers all that
		/// provide a <see cref="MVVMbasics.Attributes.MvvmNavigationTargetAttribute">MvvmNavigationTarget</see> attribute.
		/// An exception is thrown if one of the traversed Views references a Viewmodel type that has been registered
		/// already.
		/// </summary>
		/// <param name="ns">Namespace to be scanned. If ends with '.*', also sub-directories will be scanned. If NULL,
		/// the whole assembly will be scanned.</param>
		/// <param name="assembly">Assembly to be scanned. If NULL, the calling assembly will be scanned.</param>
		public void RegisterAll(string ns, Assembly assembly = null)
		{
			// If the assembly was not specified, reference the calling assembly
			if (assembly == null)
				assembly = Assembly.GetEntryAssembly();

			// List all classes that fulfill the following criteria:
			// (1) inherit from BaseView
			// (2) are located in the given namespace
			// (3) have a BindToViewmodel attribute defined
			IEnumerable<Type> views;
			if (ns != null)
				views = from t in assembly.GetTypes()
						where
							t.Namespace != null
							&& t.IsClass
							&& t.IsSubclassOf(typeof(BaseView))
							&& (t.Namespace.Equals(ns)
								|| (ns.EndsWith(".*") && t.Namespace.StartsWith(ns.Replace(".*", String.Empty))))
							&& t.GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), true).Length > 0
						select t;
			else
				views = from t in assembly.GetTypes()
						where
							t.IsClass
							&& t.IsSubclassOf(typeof(BaseView))
							&& t.GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), false).Length > 0
						select t;

			// For all of theses classes, retrieve the viewmodel type and register the mapping
			foreach (var view in views)
			{
				Type viewmodel = ((MvvmNavigationTargetAttribute)view.GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), true).First()).GetViewmodel();
				if (viewmodel != null)
				{
					if (_mappings.ContainsKey(viewmodel))
					{
						throw new ViewmodelRegistrationException(
							"Viewmodel of type '" + viewmodel.FullName + "' is already registered to a View.");
					}
					else
					{
						_mappings.Add(viewmodel, view);
					}
				}
			}
		}

		#endregion

		#region Retrieve and Navigate Methods

		/// <summary>
		/// Returns a View or throws an exception if no matching View has been registered.
		/// </summary>
		public object Retrieve<T>() where T : BaseViewmodel
		{
			if (!_mappings.ContainsKey(typeof(T)))
				throw new ViewNotFoundException();
			else
				return _mappings[typeof(T)];
		}

		/// <summary>
		/// Retrieves a View and navigates to the corresponding window.
		/// </summary>
		public void NavigateTo<T>(params Parameter[] parameters) where T : BaseViewmodel
		{
			NavigateTo<T>(new ParameterList(parameters));
		}

		/// <summary>
		/// Retrieves a View and navigates to the corresponding window, passing one parameter.
		/// </summary>
		public void NavigateTo<T>(string key, object value) where T : BaseViewmodel
		{
			NavigateTo<T>(new Parameter(key, value));
		}

		/// <summary>
		/// Retrieves a View and navigates to the corresponding window.
		/// </summary>
		public void NavigateTo<T>(ParameterList parameters) where T : BaseViewmodel
		{
			if (_mappings.ContainsKey(typeof(T)))
			{
				// If the currently active Viewmodel is registered in the Application class, call the OnNavigatedFrom
				// event on this Viewmodel!
				var application = Application.Current as BaseApplication;
				if (application != null)
				{
					BaseViewmodel currentViewmodel = application.CurrentViewmodel;
					if (currentViewmodel != null)
					{
						currentViewmodel.OnNavigatedFrom(ViewState.Deactivated);
					}
				}

				// Now actually instantiate and open the new window
				BaseView window = (BaseView)Activator.CreateInstance(_mappings[typeof(T)]);
				_windowStack.Add(window);
				window.Initialize(parameters);
				window.ShowDialog(); // Returns only after the new window has been closed

				// After the new window has been closed (either through NavigateBack or through the X button), pass
				// parameters to the previous window (since this is the window that will be reactivated)
				if (_windowStack.Count > 1)
				{
					var previousWindow = _windowStack[_windowStack.Count - 2] as BaseView;
					if (previousWindow != null)
					{
						previousWindow.Initialize(_backParameters);
						_backParameters = new ParameterList();
					}
				}
				else
				{
					if (Application.Current != null) // For non-WPF-applications, this is NULL and the App's main window
					                                 // can therefore not be retrieved
					                                 // (due to this, passing back parameters will not work in those
					                                 // Applications)
					{
						// If no previous window is found on the back stack, the App's main window is our target:
						var previousWindow = Application.Current.MainWindow as BaseView;
						if (previousWindow != null)
						{
							previousWindow.Initialize(_backParameters);
							_backParameters = new ParameterList();
						}
					}
				}

				// Finally remove the closing window from the back stack:
				if (_windowStack.Count > 0)
				{
					_windowStack.RemoveAt(_windowStack.Count - 1);
				}
			}
		}

		/// <summary>
		/// Navigates to the last page on the back stack.
		/// </summary>
		public void NavigateBack()
		{
			// Close the currently active window
			if (_windowStack.Count > 0)
			{
				_windowStack[_windowStack.Count - 1].Close();
			}
		}

		#endregion

		#region Methods for Back-Parameter Handling

		/// <summary>
		/// Stores parameters to be passed to the previous window on the next backward navigation event.
		/// </summary>
		/// <param name="parameters">Parameters to be stored for passing to the previos window.</param>
		public void SetBackParameters(params Parameter[] parameters)
		{
			_backParameters = new ParameterList(parameters);
		}

		/// <summary>
		/// Stores one parameter to be passed to the previous window on the next backward navigation event.
		/// </summary>
		/// <param name="key">Key of the parameter to be stored for passing to the previos window.</param>
		/// <param name="value">Value of the parameter to be stored for passing to the previos window.</param>
		public void SetBackParameters(string key, object value)
		{
			_backParameters = new ParameterList(new Parameter(key, value));
		}

		/// <summary>
		/// Stores parameters to be passed to the previous window on the next backward navigation event.
		/// </summary>
		/// <param name="parameters">Parameters to be stored for passing to the previos window.</param>
		public void SetBackParameters(ParameterList parameters)
		{
			_backParameters = parameters ?? new ParameterList();
		}

		/// <summary>
		/// Removes parameters which have been stored to be passed to the previous window on the next backward navigation
		/// event.
		/// </summary>
		public void ClearBackParameters()
		{
			_backParameters = new ParameterList();
		}

		#endregion

		#region Methods for Back-Stack Handling

		/// <summary>
		/// Checks whether a backwards navigation is possible or not (the latter ist the case if the current View is the
		/// Application's main View).
		/// </summary>
		/// <returns>TRUE if a backwards navigation is possible, FALSE otherwise</returns>
		public bool CanGoBack()
		{
			return _windowStack.Count > 0;
		}

		/// <summary>
		/// Removes the previously shown View from the back stack.
		/// </summary>
		public void RemoveBackEntry()
		{
			// The Application's main window is not listed in the _windowstack variable, therefore needs special treatment.
			if (_windowStack.Count > 1) // not concerning the main window
			{
				_windowStack[_windowStack.Count - 2].Close();
				_windowStack.RemoveAt(_windowStack.Count - 2);
			}
			else if (_windowStack.Count == 1) // removing the main window from the back stack!
			{
				var application = Application.Current as BaseApplication;
				if (application != null)
				{
					application.MainWindow.Close();
					application.MainWindow = _windowStack[0];
				}
				_windowStack.RemoveAt(0);
			}
		}

		/// <summary>
		/// Removes all entries from the back stack.
		/// </summary>
		public void ClearBackStack()
		{
			while (CanGoBack())
				RemoveBackEntry();
		}

		#endregion
	}
}
