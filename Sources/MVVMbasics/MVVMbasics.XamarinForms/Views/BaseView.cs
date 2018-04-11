/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;
using System.Linq;
using System.Reflection;
using MVVMbasics.Attributes;
using MVVMbasics.Helpers;
using MVVMbasics.Services;
using MVVMbasics.Viewmodels;
using Xamarin.Forms;

namespace MVVMbasics.Views
{
	/// <summary>
	/// Basic page all views can be derived from. Overrides page load- and unload-events and passes them to the
	/// Viewmodel, if any Viewmodel has been registered.
	/// </summary>
	public abstract class BaseView : ContentPage
	{
		#region Members

		/// <summary>
		/// Viewmodel instance that acts as this View's <c>DataContext</c>.
		/// </summary>
		public BaseViewmodel Viewmodel = null;

		/// <summary>
		/// Parameters that have been passed to this View. (Only stored until the next 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> event, cleared afterwards)
		/// </summary>
		internal ParameterList Parameters { get; set; }

		/// <summary>
		/// Contains a global reference to the <see cref="MVVMbasics.Services.NavigatorService">NavigatorService</see>.
		/// </summary>
		private NavigatorService NavigatorServiceReference
		{
			get
			{
				// Ensure that the navigator service reference is retrieved only once and stored in a private field:
				if (_navigatorServiceReference == null)
				{
					var app = Application.Current as BaseApplication;
					if (app != null)
					{
						try
						{
#pragma warning disable 618
							if (app.ServiceLocator != null && app.ServiceLocator.Contains<INavigatorService>())
								_navigatorServiceReference = app.ServiceLocator.Retrieve<INavigatorService>() as NavigatorService;
							else
								_navigatorServiceReference = app.Resolve(typeof(INavigatorService)) as NavigatorService;
#pragma warning restore 618
						}
						catch (Exception)
						{
							_navigatorServiceReference = null;
						}
					}
				}
				return _navigatorServiceReference;
			}
		}
		private NavigatorService _navigatorServiceReference;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseView()
		{
			Parameters = new ParameterList();
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Internally used method that tries to find the Viewmodel registered to the View, and instantiates it if
		/// necessary.
		/// </summary>
		private void InstantiateViewmodel()
		{
			// If Viewmodel is already known by the View, nothing to do here...
			if (Viewmodel == null)
			{
				// ...otherwise, if DataContext is not set yet and if we are navigating backwards, set the DataContext
				// to the Viewmodel instance that has been kept within the back stack
				if (BindingContext == null)
				{
					if (IsMvvmBasicsBackNavigation())
					{
						if (NavigatorServiceReference != null)
						{
							BindingContext = NavigatorServiceReference.BackNavigationViewmodel;
							NavigatorServiceReference.BackNavigationViewmodel = null;
						}
					}
				}

				// If a Viewmodel has been registered to this View, and if it has not been referenced as DataContext,
				// then get the Viewmodel type from the MvvmNavigationTarget attribute, instantiate it, pass a
				// reference to the ServiceLocator if requested, and set this instance as the View's DataContext
				var attribute = this.GetType().GetTypeInfo().GetCustomAttribute<MvvmNavigationTargetAttribute>(false);
				if (BindingContext == null && attribute != null)
				{
					Type viewmodelType = attribute.GetViewmodel();
					object instance = null;

					var application = Application.Current as BaseApplication;
					if (application != null)
					{
						//TODO: remove deprecated
						// If a ServiceLocator has been registered in the BaseApplication, and if the Viewmodel does not yet
						// know about it:
						// Inject ServiceLocator into Viewmodel (it will automatically be notified about the
						// ServiceLocator's availability by the ServiceLocator's setter method).
						if (viewmodelType.GetTypeInfo().DeclaredConstructors.Any(c =>
							c.GetParameters().Count() == 1 &&
							c.GetParameters()[0].ParameterType == typeof(ServiceLocator)))
						{
#pragma warning disable 618
							instance = Activator.CreateInstance(viewmodelType, application.ServiceLocator);
#pragma warning restore 618
						}
						else
						{
							instance = application.Resolve(viewmodelType);
						}
					}
					if (instance == null && viewmodelType.GetTypeInfo().DeclaredConstructors.Any(c => !c.GetParameters().Any()))
					{
						instance = Activator.CreateInstance(viewmodelType);
					}

					if (instance != null)
						BindingContext = instance;
				}

				// If this View's DataContext has been set (either by the user, or by the code snippet above), we assume
				// that the DataContext is an instance of the Viewmodel and store it
				if (BindingContext != null)
				{
					Viewmodel = (BaseViewmodel)BindingContext;

					// If not done already, inject a reference to the global dispatcher helper into the Viewmodel
					if (Viewmodel.DispatcherHelper == null)
					{
						Viewmodel.DispatcherHelper = new DispatcherHelper();
					}

					//TODO: remove deprecated
					// If a ServiceLocator has been registered in the BaseApplication, and if the Viewmodel does not yet
					// know about it:
					// Inject ServiceLocator into Viewmodel (it will automatically be notified about the
					// ServiceLocator's availability by the ServiceLocator's setter method).
#pragma warning disable 618
					var application = Application.Current as BaseApplication;
					if (application != null)
					{
						if (Viewmodel.ServiceLocator == null && application.ServiceLocator != null)
						{
							Viewmodel.ServiceLocator = application.ServiceLocator;
						}
					}
#pragma warning restore 618
				}
			}
		}

		/// <summary>
		/// Checks whether we are navigating back or not.
		/// (Checking for the built-in NavigationMode.Back is not sufficient in most cases, because we're managing the
		///  back stack on our own instead of relying on the built-in back stack and navigation.)
		/// </summary>
		/// <returns>TRUE if NavigatorService is currently closing a View and performing a backwards navigation, FALSE
		/// otherwise</returns>
		private bool IsMvvmBasicsBackNavigation()
		{
			if (NavigatorServiceReference != null)
			{
				if (NavigatorServiceReference.IsBackNavigation)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Event handler methods

		/// <summary>
		/// Overrides the pages OnAppearing event, calls the Viewmodel's 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> method and passes the
		/// parameters that were defined in the original URI.
		/// </summary>
		protected override void OnAppearing ()
		{
			InstantiateViewmodel();

			var app = Application.Current as BaseApplication;
			if (app != null)
				app.CurrentView = this;

			if (Parameters == null)
				Parameters = new ParameterList();
			if (Viewmodel != null)
			{
				MVVMbasics.Services.ViewState viewState;
				if (IsMvvmBasicsBackNavigation())
					viewState = MVVMbasics.Services.ViewState.Reactivated;
				else
					viewState = MVVMbasics.Services.ViewState.Activated;
				Viewmodel.OnNavigatedTo(new ParameterList(), Parameters, viewState);
			}

			// Clear the existing list of parameters, otherwise they would again be passed during the next OnNavigatedTo
			// event
			Parameters.Clear();

			base.OnAppearing();
		}

		protected override void OnDisappearing ()
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				MVVMbasics.Services.ViewState viewState;
				if (IsMvvmBasicsBackNavigation())
				{
					viewState = MVVMbasics.Services.ViewState.Closed; // switching to the previous page inside the App
				}
				else
				{
					viewState = MVVMbasics.Services.ViewState.Deactivated; // switching to another page inside the App
				}
				Viewmodel.OnNavigatedFrom(viewState);
			}

			base.OnDisappearing();
		}

		/// <summary>
		/// Gets called when the Application is suspended. Calls the Viewmodel's
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedFrom">OnNavigatedFrom</see> method with the
		/// appropriate type of ViewState.
		/// </summary>
		internal void OnSuspending()
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				Viewmodel.OnNavigatedFrom(MVVMbasics.Services.ViewState.Hibernated);
			}
		}

		/// <summary>
		/// Gets called when the Application is resumed after being suspended. Calls the Viewmodel's
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> method with the
		/// appropriate type of ViewState.
		/// </summary>
		internal void OnResuming()
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				Viewmodel.OnNavigatedTo(new ParameterList(), new ParameterList(), MVVMbasics.Services.ViewState.Awakened);
			}
		}

		#endregion

		#region EventToCommand implementation

		/// <summary>
		/// Generic event handler that can be used by any visual control to route the desired event to a Viewmodel Command.
		/// In addition to the <c>EventToCommand</c> event handler, the control must specify the attached property
		/// <see cref="MVVMbasics.Helpers.Event">Command</see>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected void EventToCommand(object sender, EventArgs args)
		{
			var element = sender as VisualElement;
			if (element != null)
			{
				// Check whether the sender has a Command defined in XAML
				var command = Event.GetCommand(element);
				if (command != null)
				{
					// If yes, check whether the sender has a CommandParameter defined in XAML (otherwise, we pass the
					// event args as parameter)
					object parameter = Event.GetCommandParameter(element);
					if (parameter == default(object))
						parameter = args;

					// Now evaluate this Command's CanExecute condition and, if allowed, invoke the Command
					if (command.CanExecute(parameter))
						command.Execute(parameter);
				}
			}
		}

		#endregion
	}
}
