/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using MVVMbasics.Attributes;
using MVVMbasics.Helpers;
using MVVMbasics.Services;
using MVVMbasics.Viewmodels;

namespace MVVMbasics.Views
{
	/// <summary>
	/// Basic PhoneApplicationPage all views can be derived from. Overrides page load- and unload-events and passes them
	/// to the Viewmodel, if any Viewmodel has been registered.
	/// </summary>
	public abstract class BaseView : PhoneApplicationPage
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
		/// Parameters that are stored to be passed on to the previous View during the next 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatingFrom</see> event.
		/// </summary>
		internal ParameterList BackParameters { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseView()
		{
			Parameters = new ParameterList();
			BackParameters = new ParameterList();
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
				// If a Viewmodel has been registered to this View, and if it has not been referenced as DataContext,
				// then get the Viewmodel type from the MvvmNavigationTarget attribute, instantiate it, pass a
				// reference to the ServiceLocator if requested, and set this instance as the View's DataContext
				if (DataContext == null && this.GetType().GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), false).Length > 0)
				{
					Type viewmodelType =
						((MvvmNavigationTargetAttribute)this.GetType().GetCustomAttributes(typeof(MvvmNavigationTargetAttribute), false).First())
							.GetViewmodel();
					object instance = null;

					var application = Application.Current as BaseApplication;
					if (application != null)
					{
						//TODO: remove deprecated
						// If a ServiceLocator has been registered in the BaseApplication, and if the Viewmodel does not yet
						// know about it:
						// Inject ServiceLocator into Viewmodel (it will automatically be notified about the
						// ServiceLocator's availability by the ServiceLocator's setter method).
						if (viewmodelType.GetConstructors().Any(c =>
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
					if (instance == null && viewmodelType.GetConstructors().Any(c => !c.GetParameters().Any()))
					{
						instance = Activator.CreateInstance(viewmodelType);
					}

					if (instance != null)
						DataContext = instance;
				}

				// If this View's DataContext has been set (either by the user, or by the code snippet above), we assume
				// that the DataContext is an instance of the Viewmodel and store it
				if (DataContext != null)
				{
					Viewmodel = (BaseViewmodel) DataContext;

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

			// Retrieve the ReturnedFromTombstonedState flag to decide which ViewState to use
			bool returnedFromTombstonedState = false;
			bool returnedFromHiddenState = false;
			var application = Application.Current as BaseApplication;
			if (application != null)
			{
				returnedFromTombstonedState = application.ReturnedFromTombstonedState;
				returnedFromHiddenState = application.ReturnedFromHiddenState;
				// Reset the ReturnedFromTombstonedState and ReturnedFromHiddenState flags to avoid that the next
				// time OnNavigatedTo is called, ViewState will still be set to Awakened or Shown
				application.ReturnedFromTombstonedState = false;
				application.ReturnedFromHiddenState = false;
			}

			if (Parameters == null)
				Parameters = new ParameterList();
			if (Viewmodel != null)
			{
				ViewState viewState;
				if (e.NavigationMode == NavigationMode.New)
					viewState = ViewState.Activated;
				else
					viewState = returnedFromTombstonedState 
						? ViewState.Awakened 
						: (returnedFromHiddenState ? ViewState.Shown : ViewState.Reactivated);
				Viewmodel.OnNavigatedTo(e.Uri.RetrieveParameters(), Parameters, viewState);
			}

			// Clear the existing list of parameters, otherwise they would again be passed during the next OnNavigatedTo
			// event
			Parameters.Clear();

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
				if (e.NavigationMode == NavigationMode.Back)
				{
					viewState = ViewState.Closed;

					// If parameters for the backward navigation have been stored, pass them on to the previous page!
					if (BackParameters != null)
					{
						if (BackParameters.Count > 0)
						{
							PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
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
				else
				{
					if (!e.IsNavigationInitiator)
						viewState = ViewState.Hibernated; // switching to another App or to the start screen
					else
						viewState = ViewState.Deactivated; // switching to another page inside the App
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
				if (e.NavigationMode == NavigationMode.Back)
				{
					viewState = ViewState.Closed;
				}
				else
				{
					if (!e.IsNavigationInitiator)
						viewState = ViewState.Hibernated; // switching to another App or to the start screen
					else
						viewState = ViewState.Deactivated; // switching to another page inside the App
				}
				Viewmodel.OnNavigatedFrom(viewState);
			}

			base.OnNavigatedFrom(e);
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

		#endregion
	}
}
