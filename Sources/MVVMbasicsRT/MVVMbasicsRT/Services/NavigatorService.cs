/*
 * (c) 2013-2015 Andreas Kuntner
 * 
 * some corrections Kristian Walsh
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MVVMbasics.Attributes;
using MVVMbasics.Exceptions;
using MVVMbasics.Viewmodels;
using MVVMbasics.Views;

namespace MVVMbasics.Services
{
	/// <summary>
	/// Service which provides page navigation functionality. Includes both "standard" navigation methods which navigate
	/// backwards, as well as a View-/Viewmodel-Locator that stores mappings of View to Viewmodel and allows to directly
	/// navigate to a View that is associated with a given Viewmodel.
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
		private readonly Dictionary<Type, TypeInfo> _mappings = new Dictionary<Type, TypeInfo>();

		/// <summary>
		/// Collection of Viewmodel instances of all Views that have been opened and not closed yet. When navigating away
		/// from a View (while not closing it), it's Viewmodel instance is added to this stack, to be retrieved and
		/// re-assgined to the View whenever the View is reactivated again.
		/// </summary>
		private readonly List<BaseViewmodel> _backStack = new List<BaseViewmodel>();

		/// <summary>
		/// Flag that indicates whether NavigatorService is closing a view and navigating backwards at the moment. This is
		/// read by BaseView's OnNavigatedFrom and OnNavigatedTo methods.
		/// </summary>
		internal bool IsBackNavigation = false;

		/// <summary>
		/// If we are currently navigating backwards (as indicated by the IsBackNavigation flag), this property contains
		/// the Viewmodel instance of the View that will be reactivated as result of the current backwards navigation, in
		/// order to be re-assigned to this View. Re-assigning is done by the View itself, afterwards this property is
		/// cleared and contains NULL again.
		/// </summary>
		internal BaseViewmodel BackNavigationViewmodel = null;

		#endregion

		#region Register Methods

		/// <summary>
		/// Registers a mapping of Viewmodel to View. Both Viewmodel and View must be supplied as <c>Type</c> objects.
		/// The Viewmodel type must be a subtype of <see cref="MVVMbasics.Viewmodels.BaseViewmodel">BaseViewmodel</see>,
		/// the View type must be a subtype of <see cref="BaseView">BaseView</see>.
		/// Exceptions will be thrown if these type constraints are not fulfilled, or if a mapping with the specified
		/// Viewmodel type exists already.
		/// </summary>
		public void Register<T>(object view) where T : BaseViewmodel
		{
			if (!(view is Type))
				throw new ViewmodelRegistrationException("View to be registered must be supplied as type 'Type'.");
			if (!(((Type)view).GetTypeInfo().IsSubclassOf(typeof(BaseView))))
				throw new ViewmodelRegistrationException("Viewmodel may only be registered to View that extends 'BaseView'.");
			if (_mappings.ContainsKey(typeof(T)))
				throw new ViewmodelRegistrationException("Viewmodel of type '" + typeof(T).FullName +
														 "' is already registered to a View.");
			Type type = (Type)view;
			_mappings.Add(typeof(T), type.GetTypeInfo());
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
			// (The method GetCallingAssembly is not available in Windows Store Libraries, therefore must be called
			// through reflection)
			if (assembly == null)
				assembly = (Assembly)typeof(Assembly).GetTypeInfo().GetDeclaredMethod("GetCallingAssembly")
					.Invoke(null, new object[0]);

			// List all classes that fulfill the following criteria:
			// (1) inherit from BaseView
			// (2) are located in the given namespace
			// (3) have a BindToViewmodel attribute defined
			IEnumerable<TypeInfo> views;
			if (ns != null)
				views = from t in assembly.DefinedTypes
						where
							t.Namespace != null
							&& t.IsClass
							&& t.IsSubclassOf(typeof(BaseView))
							&& (t.Namespace.Equals(ns)
								|| (ns.EndsWith(".*") && t.Namespace.StartsWith(ns.Replace(".*", String.Empty))))
							&& t.GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), true).Any()
						select t;
			else
				views = from t in assembly.DefinedTypes
						where
							t.IsClass
							&& t.IsSubclassOf(typeof(BaseView))
							&& t.GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), false).Any()
						select t;

			// For all of theses classes, retrieve the viewmodel type and register the mapping
			foreach (var view in views)
			{
				var navigationTarget = view.GetCustomAttribute<MvvmNavigationTargetAttribute>(true);
				if (navigationTarget != null)
				{
					Type viewmodel = navigationTarget.GetViewmodel();
					if (viewmodel != null)
					{
						if (_mappings.ContainsKey(viewmodel))
						{
							throw new ViewmodelRegistrationException("Viewmodel of type '" + viewmodel.FullName +
																	 "' is already registered to a View.");
						}
						else
						{
							_mappings.Add(viewmodel, view);
						}
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
			return Retrieve(typeof(T));
		}

		private object Retrieve(Type t)
		{
			if (!_mappings.ContainsKey(t))
				throw new ViewNotFoundException();
			else
				return _mappings[t];
		}

		/// <summary>
		/// Retrieves a View and navigates to the corresponding page.
		/// </summary>
		public void NavigateTo<T>(params Parameter[] parameters) where T : BaseViewmodel
		{
			NavigateTo<T>(new ParameterList(parameters));
		}

		/// <summary>
		/// Retrieves a View and navigates to the corresponding page, passing one parameter.
		/// </summary>
		public void NavigateTo<T>(string key, object value) where T : BaseViewmodel
		{
			NavigateTo<T>(new Parameter(key, value));
		}

		/// <summary>
		/// Retrieves a View and navigates to the corresponding page.
		/// </summary>
		public void NavigateTo<T>(ParameterList parameters) where T : BaseViewmodel
		{
			var application = Application.Current as BaseApplication;
			if (application != null)
			{
				Frame frame = application.RootFrame;
				if (frame != null)
				{
					TypeInfo type = (TypeInfo)Retrieve<T>();
					frame.Navigated += (sender, args) =>
						{
							var nextView = args.Content as BaseView;
							if (nextView != null)
							{
								nextView.Parameters = parameters;
							}
						};
					IsBackNavigation = false;
					frame.Navigate(type.AsType());
				}
			}
		}

		/// <summary>
		/// Navigates to the last page on the back stack.
		/// </summary>
		public void NavigateBack()
		{
			if (_backStack.Count > 0)
			{
				var application = Application.Current as BaseApplication;
				if (application != null)
				{
					Frame frame = application.RootFrame;
					if (frame != null)
					{
						// Since we manage the back stack on our own instead of relying on the built-in back stack and
						// navigation, we cannot simply call frame.NavigateBack()
						// Instead, retrieve the Viewmodel instance of the View that is about to be reactivated from the
						// back stack, store it in the BackNavigationViewmodel property for the page to pick it up, call
						// frame.Navigate() and remove the retrieved Viewmodel instance from the back stack.
						BaseViewmodel previousViewmodel = _backStack[_backStack.Count - 1];
						TypeInfo type = (TypeInfo)Retrieve(previousViewmodel.GetType());
						IsBackNavigation = true;
						BackNavigationViewmodel = previousViewmodel;

						// Keep the system page stack correctly sized
						for (int i=0; i<2 && frame.BackStack.Count > 0; ++i)
						{
							// Remove two pages: the one we're leaving, and the one we're going back to (which will be
							// added to the stack by frame.Navigate(...))
							frame.BackStack.RemoveAt(frame.BackStack.Count - 1); 
						}

						// Now, actually do navigate...
						frame.Navigate(type.AsType());

						// ...and keep the internal back stack clear
						_backStack.RemoveAt(_backStack.Count - 1);
					}
				}
			}
		}

		#endregion

		#region Methods for Back-Parameter Handling

		/// <summary>
		/// Stores parameters to be passed to the previous page on the next backward navigation event.
		/// </summary>
		/// <param name="parameters">Parameters to be stored for passing to the previos page.</param>
		public void SetBackParameters(params Parameter[] parameters)
		{
			SetBackParameters(new ParameterList(parameters));
		}

		/// <summary>
		/// Stores one parameter to be passed to the previous page on the next backward navigation event.
		/// </summary>
		/// <param name="key">Key of the parameter to be stored for passing to the previos page.</param>
		/// <param name="value">Value of the parameter to be stored for passing to the previos page.</param>
		public void SetBackParameters(string key, object value)
		{
			SetBackParameters(new ParameterList(new Parameter(key, value)));
		}

		/// <summary>
		/// Stores parameters to be passed to the previous page on the next backward navigation event.
		/// </summary>
		/// <param name="parameters">Parameters to be stored for passing to the previos page.</param>
		public void SetBackParameters(ParameterList parameters)
		{
			var application = Application.Current as BaseApplication;
			if (application != null)
			{
				Frame frame = application.RootFrame;
				if (frame != null)
				{
					BaseView view = (BaseView)frame.Content;
					if (view != null)
					{
						view.BackParameters = parameters;
					}
				}
			}
		}

		/// <summary>
		/// Removes parameters which have been stored to be passed to the previous page on the next backward navigation
		/// event.
		/// </summary>
		public void ClearBackParameters()
		{
			var application = Application.Current as BaseApplication;
			if (application != null)
			{
				Frame frame = application.RootFrame;
				if (frame != null)
				{
					BaseView view = (BaseView)frame.Content;
					if (view != null)
					{
						view.BackParameters = new ParameterList();
					}
				}
			}
		}

		#endregion

		#region Methods for Back-Stack Handling

		/// <summary>
		/// Stores the currently closing View's Viewmodel instance on the back stack, in order to be able to re-assign
		/// it whenever this View will be reactivated again.
		/// Gets called from within BaseView's OnNavigatingFrom method.
		/// </summary>
		/// <param name="viewmodel">Viewmodel instance of the View that is currently being closed</param>
		internal void AddToBackstack(BaseViewmodel viewmodel)
		{
			_backStack.Add(viewmodel);
		}

		/// <summary>
		/// Checks whether a backwards navigation is possible or not (the latter ist the case if the current View is the
		/// Application's main View).
		/// </summary>
		/// <returns>TRUE if a backwards navigation is possible, FALSE otherwise</returns>
		public bool CanGoBack()
		{
			return _backStack.Count > 0;
		}

		/// <summary>
		/// Removes the previously shown View from the back stack.
		/// </summary>
		public void RemoveBackEntry()
		{
			if (_backStack.Count > 0)
				_backStack.RemoveAt(_backStack.Count - 1);
		}

		/// <summary>
		/// Removes all entries from the back stack.
		/// </summary>
		public void ClearBackStack()
		{
			_backStack.Clear();
		}

		#endregion
	}
}
