/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Phone.Controls;
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
		/// Stores all mapping of Viewmodels (keys in the <c>Dictionary</c>) to Views (pages, stored as values in the
		/// <c>Dictionary</c>). For Viewmodels, only their types are stored instead of actual instances. For Views,
		/// their URIs are stored instaad of instances.
		/// </summary>
		private readonly Dictionary<Type, Uri> _mappings = new Dictionary<Type, Uri>();

		#endregion

		#region Register Methods

		/// <summary>
		/// Registers a mapping of Viewmodel to View. Viewmodel must be supplied as <c>Type</c> object, View must be
		/// supplied as <c>Uri</c> object. The Viewmodel type  must be a subtype of
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel">BaseViewmodel</see>.
		/// Exceptions will be thrown if this type constraint is not fulfilled, or if a mapping with the specified
		/// Viewmodel type exists already.
		/// </summary>
		public void Register<T>(object view) where T : BaseViewmodel
		{
			if (!(view is Uri))
				throw new ViewmodelRegistrationException("Viewmodel may only be registered to View of type Uri.");
			if (_mappings.ContainsKey(typeof(T)))
				throw new ViewmodelRegistrationException("Viewmodel of type '" + typeof(T).FullName +
														 "' is already registered to a View.");
			Uri uri = (Uri)view;
			_mappings.Add(typeof(T), uri);
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
				assembly = Assembly.GetCallingAssembly();

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
				var navigationTarget = (MvvmNavigationTargetAttribute)view.GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), true).First();
				Type viewmodel = navigationTarget.GetViewmodel();
				if (viewmodel != null)
				{
					if (_mappings.ContainsKey(viewmodel))
					{
						throw new ViewmodelRegistrationException(
							"Viewmodel of type '" + viewmodel.FullName + "' is already registered to a View.");
					}
					else
					{
						string uriString = String.Empty;
						string assemblyName = new AssemblyName(assembly.FullName).Name;
						if (navigationTarget.HasPath())
						{
							// If the exact path of the View is registered in the MvvmNavigationTarget Attribute, use
							// this as URI (after removing assembly name and adding file name, if necessary)
							uriString = navigationTarget.GetPath();
							uriString = uriString.Replace(assemblyName, String.Empty);
							uriString = uriString.Replace("//", "/");
							if (!uriString.EndsWith(".xaml"))
							{
								if (!uriString.EndsWith("/"))
									uriString += "/";
								uriString += view.Name + ".xaml";
							}
							if (!uriString.StartsWith("/"))
								uriString = "/" + uriString;
						}
						else
						{
							// If the exact path of the View is not registered in the MvvmNavigationTarget Attribute,
							// use the following workaround:
							// Since the URI can not be retrieved from the View class, we assume that the file path
							// equals to the namespace - just remove the assembly name from the namespace, replace .
							// by / and add .xaml to the end!
							string actualNs = view.Namespace;
							if (String.IsNullOrEmpty(actualNs) || actualNs.Equals(assemblyName))
							{
								uriString = "/" + view.Name + ".xaml";
							}
							else
							{
								actualNs = actualNs.Replace(assemblyName, String.Empty);
								if (actualNs.StartsWith("."))
									actualNs = actualNs.Substring(1);
								actualNs = actualNs.Replace('.', '/');
								uriString = "/" + actualNs + "/" + view.Name + ".xaml";
							}
						}
						Uri uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
						_mappings.Add(viewmodel, uri);
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
			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				Uri uri = (Uri)Retrieve<T>();
				frame.Navigated += (sender, args) =>
					{
						var nextView = args.Content as BaseView;
						if (nextView != null)
						{
							nextView.Parameters = parameters;
						}
					};
				frame.Navigate(new Uri(uri.ToString(), UriKind.RelativeOrAbsolute));
			}
		}

		/// <summary>
		/// Navigates to the last page on the back stack.
		/// </summary>
		public void NavigateBack()
		{
			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
				if (page != null)
				{
					if (page.NavigationService.CanGoBack)
						page.NavigationService.GoBack();
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
			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				BaseView view = (BaseView)frame.Content;
				if (view != null)
				{
					view.BackParameters = parameters;
				}
			}
		}

		/// <summary>
		/// Removes parameters which have been stored to be passed to the previous page on the 
		/// next backward navigation event.
		/// </summary>
		public void ClearBackParameters()
		{
			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				BaseView view = (BaseView)frame.Content;
				if (view != null)
				{
					view.BackParameters = new ParameterList();
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
			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
				if (page != null)
				{
					return page.NavigationService.CanGoBack;
				}
			}
			return false;
		}

		/// <summary>
		/// Removes the previously shown View from the back stack.
		/// </summary>
		public void RemoveBackEntry()
		{
			PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (frame != null)
			{
				PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
				if (page != null)
				{
					if (page.NavigationService.CanGoBack)
						page.NavigationService.RemoveBackEntry();
				}
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
