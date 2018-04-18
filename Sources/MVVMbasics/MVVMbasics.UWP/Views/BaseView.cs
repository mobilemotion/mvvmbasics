/*
 * (c) 2013-2018 Andreas Kuntner
 * 
 * some corrections Kristian Walsh
 */

using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MVVMbasics.Attributes;
using MVVMbasics.Helpers;
using MVVMbasics.Services;
using MVVMbasics.Viewmodels;

namespace MVVMbasics.Views
{
	/// <summary>
	/// Basic page all views can be derived from. Overrides page load- and unload-events and passes them to the
	/// Viewmodel, if any Viewmodel has been registered.
	/// </summary>
	public abstract class BaseView : Page
	{
		#region Members

		/// <summary>
		/// Flag indicating whether the App is currently minimized
		/// </summary>
		private bool _hidden = false;

		//TODO: remove deprecated
		/// <summary>
		/// Viewmodel instance that acts as this View's <c>DataContext</c>.
		/// </summary>
		[Obsolete(@"Implement the IBindableView interface and use the Vm property instead")]
		public BaseViewmodel Viewmodel = null;

		/// <summary>
		/// Parameters that have been passed to this View. (Only stored until the next 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> event, cleared afterwards)
		/// </summary>
		internal ParameterList Parameters { get; set; }

		/// <summary>
		/// Parameters that are stored to be passed on to the previous View during the next 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatingFrom</see> event.
		/// </summary>
		internal ParameterList BackParameters { get; set; }

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
			BackParameters = new ParameterList();

			DataContextChanged += View_DataContextChanged;

			InstantiateViewmodel();
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
			if (Viewmodel != null)
				return;

			// ...otherwise, if DataContext is not set yet and if we are navigating backwards, set the DataContext
			// to the Viewmodel instance that has been kept within the back stack
			if (DataContext == null)
			{
				if (IsMvvmBasicsBackNavigation())
				{
					if (NavigatorServiceReference != null)
					{
						DataContext = NavigatorServiceReference.BackNavigationViewmodel;
						NavigatorServiceReference.BackNavigationViewmodel = null;
					}
				}
			}

			// If a Viewmodel has been registered to this View, and if it has not been referenced as DataContext,
			// then get the Viewmodel type from the MvvmNavigationTarget attribute or the IBindableView interface,
			// instantiate it, pass a reference to the ServiceLocator if requested, and set this instance as the
			// View's DataContext
			if (DataContext == null)
			{
				Type viewmodelType = null;
				var attribute = this.GetType().GetTypeInfo().GetCustomAttribute<MvvmNavigationTargetAttribute>(false);
				if (attribute != null)
				{
					viewmodelType = attribute.GetViewmodel();
				}
				else
				{
					var @interface = this.GetType().GetTypeInfo().ImplementedInterfaces
						.FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IBindableView<>));
					if (@interface != null)
					{
						viewmodelType = @interface.GenericTypeArguments.FirstOrDefault();
					}
				}

				if (viewmodelType != null)
				{
					object instance = null;

					var application = Application.Current as BaseApplication;
					if (application != null)
					{
						//TODO: remove deprecated
						// If a ServiceLocator has been registered in the BaseApplication, and if the Viewmodel does
						// not yet know about it:
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
						DataContext = instance;
				}
			}
		}

		/// <summary>
		/// Whenever the view's DataContext changes, set its Viewmodel and (if implementing the <code>IBindableView</code>
		/// interface) Vm properties accordingly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void View_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (DataContext == null)
				return;

			// Retrieve the current DataContext value - this is the Viewmodel instance
			Viewmodel = (BaseViewmodel)DataContext;

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

			// If implementing the IBindableView interface, also store the current viewmodel in the view's Vm property
			if (this.GetType().GetTypeInfo().ImplementedInterfaces
				.Any(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IBindableView<>)))
			{
				var vmProperty = this.GetType().GetRuntimeProperty("Vm");

				if (vmProperty != null)
				{
					vmProperty.SetMethod.Invoke(this, new[] { DataContext });
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
		/// Overrides the pages OnNavigatedTo event, calls the Viewmodel's 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> method and passes the
		/// parameters that were defined in the original URI.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			InstantiateViewmodel();

			// Store a reference to the current View's main frame within the Application
			// (this reference is needed by the NavigatorService)
			var application = Application.Current as BaseApplication;
			if (application != null)
			{
				application.RootFrame = this.Frame;
				application.CurrentView = this;
			}

			if (Parameters == null)
				Parameters = new ParameterList();
			if (Viewmodel != null)
			{
				ViewState viewState;
				if (e.NavigationMode != NavigationMode.New || IsMvvmBasicsBackNavigation())
					viewState = ViewState.Reactivated;
				else
					viewState = ViewState.Activated;
				Viewmodel.OnNavigatedTo(new ParameterList(), Parameters, viewState);
			}

			// Clear the existing list of parameters, otherwise they would again be passed during the next OnNavigatedTo
			// event
			Parameters.Clear();

			// This is necessary to avoid crashing Visual Studio's visual XAML editor
			if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
				Window.Current.VisibilityChanged += OnWindowVisibilityChanged;

			base.OnNavigatedTo(e);
		}

		/// <summary>
		/// Overrides the page's OnNavigatingFrom method and calls the Viewmodel's
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.CancelNavigatingFrom">CancelNavigatingFrom</see> method,
		/// allowing the actual (derived) window class to cancel the navigation.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				ViewState viewState;
				if (e.NavigationMode == NavigationMode.Back || IsMvvmBasicsBackNavigation())
				{
					viewState = ViewState.Closed;

					// If parameters for the backward navigation have been stored, pass them on to the previous page!
					if (BackParameters != null)
					{
						if (BackParameters.Count > 0)
						{
							var application = Application.Current as BaseApplication;
							if (application != null)
							{
								Frame frame = application.RootFrame;
								if (frame != null)
								{
									frame.Navigated += (sender, args) =>
									{
										var previousView = args.Content as BaseView;
										if (previousView != null)
										{
											previousView.Parameters = BackParameters;
										}
									};
								}
							}
						}
					}
				}
				else
				{
					viewState = ViewState.Deactivated; // switching to another page inside the App

					if (NavigatorServiceReference != null)
					{
						// Store the current Viewmodel's instance on the back stack
						NavigatorServiceReference.AddToBackstack(Viewmodel);
					}
				}
				e.Cancel = Viewmodel.CancelNavigatingFrom(viewState);
			}

			base.OnNavigatingFrom(e);
		}

		/// <summary>
		/// Overrides the page's OnNavigatedFrom method and calls the Viewmodel's
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedFrom">OnNavigatedFrom</see> method.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				ViewState viewState;
				if (e.NavigationMode == NavigationMode.Back || IsMvvmBasicsBackNavigation())
				{
					viewState = ViewState.Closed; // switching to the previous page inside the App
				}
				else
				{
					viewState = ViewState.Deactivated; // switching to another page inside the App
				}
				Viewmodel.OnNavigatedFrom(viewState);
			}

			// Ensure that the page can correctly be disposed
			DataContextChanged -= View_DataContextChanged;
			Viewmodel = null;
			DataContext = null;

			Window.Current.VisibilityChanged -= OnWindowVisibilityChanged;

			base.OnNavigatedFrom(e);
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
				Viewmodel.OnNavigatedFrom(ViewState.Hibernated);
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
				Viewmodel.OnNavigatedTo(new ParameterList(), new ParameterList(), ViewState.Awakened);
			}
		}

		/// <summary>
		/// Overrides the window's VisibilityChanged event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWindowVisibilityChanged(object sender, VisibilityChangedEventArgs e)
		{
			var application = Application.Current as BaseApplication;
			if (application != null)
			{
				if (application.CurrentView.Equals(this))
				{
					InstantiateViewmodel();
					if (Viewmodel != null)
					{
						if (!e.Visible && !_hidden)
						{
							_hidden = true;
							Viewmodel.OnNavigatedFrom(ViewState.Hidden); // switching to another App or to the start screen
						}
						else if (_hidden)
						{
							_hidden = false;
							Viewmodel.OnNavigatedTo(new ParameterList(), new ParameterList(), ViewState.Shown);
						}
					}
				}
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
			var element = sender as UIElement;
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

		/// <summary>
		/// Generic routed event handler that can be used by any visual control to route the desired routed event to a
		/// Viewmodel Command. In addition to the <c>EventToCommand</c> event handler, the control must specify the
		/// attached property <see cref="MVVMbasics.Helpers.Event">Command</see>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected void EventToCommand(object sender, RoutedEventArgs args)
		{
			var element = sender as UIElement;
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
