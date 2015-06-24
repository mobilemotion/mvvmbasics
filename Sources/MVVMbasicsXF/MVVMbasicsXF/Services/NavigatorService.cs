/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVMbasics.Attributes;
using MVVMbasics.Exceptions;
using MVVMbasics.Viewmodels;
using MVVMbasics.Views;
using Xamarin.Forms;

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
		internal readonly Dictionary<Type, Type> Mappings = new Dictionary<Type, Type>();

		/// <summary>
		/// If we are currently navigating backwards (as indicated by the IsBackNavigation flag), this property contains
		/// the Viewmodel instance of the View that will be reactivated as result of the current backwards navigation, in
		/// order to be re-assigned to this View. Re-assigning is done by the View itself, afterwards this property is
		/// cleared and contains NULL again.
		/// </summary>
		internal BaseViewmodel BackNavigationViewmodel = null;

		/// <summary>
		/// Flag that indicates whether NavigatorService is closing a view and navigating backwards at the moment. This is
		/// read by BaseView's OnNavigatedFrom and OnNavigatedTo methods.
		/// </summary>
		internal bool IsBackNavigation = false;

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
			if (Mappings.ContainsKey(typeof(T)))
				throw new ViewmodelRegistrationException("Viewmodel of type '" + typeof(T).FullName +
														 "' is already registered to a View.");
			Type type = (Type)view;
			Mappings.Add(typeof(T), type);
		}

		/// <summary>
		/// Traverses all Views that are located within a given namespace and registers all that provide a
		/// <see cref="MVVMbasics.Attributes.MvvmNavigationTarget">MvvmNavigationTarget</see> attribute.
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
		/// provide a <see cref="MVVMbasics.Attributes.MvvmNavigationTarget">MvvmNavigationTarget</see> attribute.
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
						if (Mappings.ContainsKey(viewmodel))
						{
							throw new ViewmodelRegistrationException("Viewmodel of type '" + viewmodel.FullName +
																	 "' is already registered to a View.");
						}
						else
						{
							Mappings.Add(viewmodel, view.AsType());
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
			if (!Mappings.ContainsKey(t))
				throw new ViewNotFoundException();
			else
				return Mappings[t];
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
			var app = Application.Current as BaseApplication;
			if (app != null)
			{
				Type type = (Type)Retrieve<T>();
				var nextPage = Activator.CreateInstance (type) as BaseView;
				if (nextPage != null)
				{
					nextPage.Parameters = parameters;
					app.MainPage.Navigation.PushAsync (nextPage);
				}
			}
		}

		/// <summary>
		/// Navigates to the last page on the back stack.
		/// </summary>
		public void NavigateBack()
		{
			if (CanGoBack ())
			{
				var app = Application.Current as BaseApplication;
				if (app != null)
				{
					app.MainPage.Navigation.PopAsync ();
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
			var app = Application.Current as BaseApplication;
			if (app != null)
			{
				int stackSize = app.MainPage.Navigation.NavigationStack.Count;
				if (stackSize >= 2)
				{
					var previousPage = app.MainPage.Navigation.NavigationStack[stackSize - 2] as BaseView;
					if (previousPage != null)
					{
						previousPage.Parameters = parameters;
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
			var app = Application.Current as BaseApplication;
			if (app != null)
			{
				int stackSize = app.MainPage.Navigation.NavigationStack.Count;
				if (stackSize >= 2)
				{
					var previousPage = app.MainPage.Navigation.NavigationStack[stackSize - 2] as BaseView;
					if (previousPage != null)
					{
						previousPage.Parameters = new ParameterList();
					}
				}
			}
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
			var app = Application.Current as BaseApplication;
			if (app != null)
			{
				return app.MainPage.Navigation.NavigationStack.Count > 1;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Removes the previously shown View from the back stack.
		/// </summary>
		public void RemoveBackEntry()
		{
			var app = Application.Current as BaseApplication;
			if (app != null) {
				int stackSize = app.MainPage.Navigation.NavigationStack.Count;
				if (stackSize > 2)
				{
					app.MainPage.Navigation.RemovePage (app.MainPage.Navigation.NavigationStack [stackSize - 2]);
				}
				else if (stackSize == 2)
				{
					ClearBackStack ();
				}
			}
		}

		/// <summary>
		/// Removes all entries from the back stack.
		/// </summary>
		public void ClearBackStack()
		{
			var app = Application.Current as BaseApplication;
			if (app != null)
			{
				var navigation = app.MainPage.Navigation;
				while (navigation.NavigationStack.Count >= 2)
				{
					navigation.RemovePage(navigation.NavigationStack[0]);
				}
			}
		}

		#endregion
	}
}
