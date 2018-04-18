/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;
using System.Linq;
using System.Windows;
using MVVMbasics.Attributes;
using MVVMbasics.Helpers;
using MVVMbasics.Services;
using MVVMbasics.Viewmodels;

namespace MVVMbasics.Views
{
    /// <summary>
    /// Basic WPF window all views can be derived from. Overrides window load- and unload-events and passes them to the
    /// Viewmodel, if any Viewmodel has been registered.
    /// </summary>
	public class BaseView : Window
	{
		#region Members

		/// <summary>
		/// Flag indicating whether the View is minimized (to the taskbar) or not.
		/// </summary>
	    private bool _hidden = false;

		/// <summary>
		/// Flag indicating whether the View is newly opened, or reactivated (e.g., when receiving back focus).
		/// </summary>
        private bool _opening = false;
        
		/// <summary>
		/// Viewmodel instance that acts as this View's <c>DataContext</c>.
		/// </summary>
		public BaseViewmodel Viewmodel = null;

		/// <summary>
		/// Parameters that have been passed to this View. (Only stored until the next 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see> event, cleared afterwards)
		/// </summary>
        private ParameterList _parameters = new ParameterList();

		#endregion

		#region Constructor

		/// <summary>
        /// Constructor. Registers the Initialized and Activated events.
        /// </summary>
		public BaseView()
        {
            this.Initialized += BaseView_Initialized;
            this.Activated += BaseView_Activated;
			this.StateChanged += BaseView_StateChanged;
		}

	    /// <summary>
		/// Helper method that is called directly after the constructor, to pass parameters to the View. Necessary in
		/// order to provide a parameterless constructor.
		/// </summary>
		/// <param name="parameters"></param>
		internal void Initialize(ParameterList parameters)
        {
            _parameters = parameters ?? new ParameterList();

            FireOnNavigatedTo();
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
					        c.GetParameters()[0].ParameterType == typeof (ServiceLocator)))
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

	        // In a Viewmodel has been registered to this View (either by the code snippet above, or by an earlier
			// method), register it as currently active Viewmodel in the Application
			if (Viewmodel != null)
			{
				if (Application.Current != null)
				{
					if (Application.Current is BaseApplication)
					{
						(Application.Current as BaseApplication).CurrentViewmodel = Viewmodel;
					}
				}
			}
		}

        /// <summary>
        /// Calls the Viewmodel's <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedTo">OnNavigatedTo</see>
        /// method.
        /// </summary>
        private void FireOnNavigatedTo()
        {
            InstantiateViewmodel();
            if (Viewmodel != null)
            {
                Viewmodel.OnNavigatedTo(new ParameterList(), _parameters,
                                         _opening ? ViewState.Activated : ViewState.Reactivated);
            }

			// Clear the existing list of parameters, otherwise they would again be passed on to the next OnNavigatedTo
			// call
            _parameters.Clear();
        }

		#endregion

		#region Event handler methods

		/// <summary>
        /// Overrides the window's Initialized event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseView_Initialized(object sender, EventArgs e)
        {
            _opening = true;

            if (Equals(Application.Current.MainWindow, this))
            {
                FireOnNavigatedTo();
            }
        }

        /// <summary>
        /// Overrides the window's Activated event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseView_Activated(object sender, EventArgs e)
        {
            _opening = false;
        }

        /// <summary>
		/// Overrides the window's OnClosing method and calls the Viewmodel's 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.CancelNavigatingFrom">CancelNavigatingFrom</see> method,
		/// allowing the actual (derived) window class to cancel the unload operation.
        /// </summary>
        /// <param name="e"></param>
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				e.Cancel = Viewmodel.CancelNavigatingFrom(ViewState.Closed);
			}
			
			base.OnClosing(e);
		}

        /// <summary>
		/// Overrides the window's OnClosed method and calls the Viewmodel's 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel.OnNavigatedFrom">OnNavigatedFrom</see> method.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(System.EventArgs e)
		{
			InstantiateViewmodel();
			if (Viewmodel != null)
			{
				Viewmodel.OnNavigatedFrom(ViewState.Closed);
			}

			base.OnClosed(e);
		}

		/// <summary>
		/// Is called when the window is minimized, maximized, or returns back to "normal" state. Used to inform the
		/// Viewmodel about hidden (when minimized) and shown (when returning from minimized) states.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void BaseView_StateChanged(object sender, EventArgs eventArgs)
		{
			switch (WindowState)
			{
				case WindowState.Minimized:
					_hidden = true;
					Viewmodel.OnNavigatedFrom(ViewState.Hidden);
					break;
				case WindowState.Normal:
					if (_hidden)
					{
						_hidden = false;
						Viewmodel.OnNavigatedTo(new ParameterList(), new ParameterList(), ViewState.Shown);
					}
					break;
				// WindowsState.Maximized is not relevant for this use case
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

		#endregion
	}
}
