/*
 * (c) 2013-2018 Andreas Kuntner
 */

using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MVVMbasics.Attributes;
using MVVMbasics.Commands;
using MVVMbasics.Helpers;
using MVVMbasics.Models;
using MVVMbasics.Services;
using System.Threading.Tasks;

namespace MVVMbasics.Viewmodels
{
	/// <summary>
	/// Base viewmodel class all actual viewmodels can be derived from.
	/// </summary>
	public abstract class BaseViewmodel : BaseModel, INotifyPropertyChanged
	{
		#region Private members

		/// <summary>
		/// Flag that indicates whether Commands shall be automatically mapped to methods.
		/// </summary>
		private readonly bool _commandAutobinding = false;

		#endregion

		#region Public members

		/// <summary>
		/// Flag that indicates whether the View which is registered to this Viewmodel is active (shown on the screen)
		/// or not. Can be used to decide whether a message box shall be shown or not, for example.
		/// </summary>
		public ViewState ViewState { get; private set; }

		/// <summary>
		/// Reference to the global dispatcher helper class. Used by the <code>RunOnMainThread</code> method.
		/// </summary>
		public IDispatcherHelper DispatcherHelper { get; set; }

		//TODO: remove deprecated
		/// <summary>
		/// ServiceLocator to be used to retrieve service instances during Viewmodel logic. Is set by the View when
		/// instantiating the Viewmodel, only if it was defined in the <c>BaseApplication</c>.
		/// Should be used instead of directly referencing <c>Application.Current.ServiceLocator</c>, since the
		/// Application might not in all cases be accesible (e.g., in multi-platform Apps the Viewmodel might be located
		/// within a Portable Class Library that can not access the <c>Application</c> classes.
		/// </summary>
		[Obsolete(@"Use App.Services or any 3rd party IoC container instead")]
		public ServiceLocator ServiceLocator
		{
			get { return _serviceLocator; }
			set
			{
				if (_serviceLocator == null)
				{
					_serviceLocator = value;
					OnServiceLocatorAvailable();
				}
			}
		}
		private ServiceLocator _serviceLocator = null;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor. Tries to set the <c>CommandAutobinding</c> flag depending on whether an
		/// <see cref="MVVMbasics.Attributes.MvvmCommandAutobindingAttribute">MvvmCommandAutobinding</see> attribute is
		/// registered, and automatically binds Commands to methods if applicable.
		/// </summary>
		public BaseViewmodel()
		{
			// Check if an MvvmCommandAutobinding Attribute is registered to this Viewmodel
			MvvmCommandAutobindingAttribute attribute =
				(MvvmCommandAutobindingAttribute)GetType().GetTypeInfo().GetCustomAttributes(typeof(MvvmCommandAutobindingAttribute), false)
				.FirstOrDefault();
			if (attribute != null)
			{
				_commandAutobinding = attribute.GetAutobinding();
			}

			// If CommandAutobinding is turned on, loop through all Commands and check if a method exists that the 
			// Command can be mapped to
			if (_commandAutobinding)
			{
				WireCommandAutobinding();
			}
		}

		#endregion

		#region Navigation event methods

		//TODO: remove deprecated
		/// <summary>
		/// Method that is called when the App's <see cref="MVVMbasics.Helpers.ServiceLocator">ServiceLocator</see> is
		/// set for the first time. Usually this happens shortly after the constructor.
		/// Subclasses can override this method to be notified when the <c>ServiceLocator</c> is available, and then do
		/// all the tasks that would occur in the constructor but are dependent on the <c>ServiceLocator</c>.
		/// </summary>
		[Obsolete(@"Use App.Services or any 3rd party IoC container instead")]
		public virtual void OnServiceLocatorAvailable()
		{
		}

		/// <summary>
		/// Method that is called when navigating to a page / window. Subclasses can override this method to be notified
		/// when the page / window they represent is loaded and shown on the screen. In addition, all parameters that
		/// were passed to the page / window can be easily retrieved and processed inside this method.
		/// </summary>
		/// <param name="uriParameters">List of parameters that were contained within the URI used to navigate to the current page / window.</param>
		/// <param name="parameters">List of parameters that were passed from the calling page / window.</param>
		/// <param name="viewState">Indicates the lifecycle state the View is about to reach.</param>
		public virtual void OnNavigatedTo(ParameterList uriParameters, ParameterList parameters, ViewState viewState)
		{
			ViewState = viewState;
		}

		/// <summary>
		/// Method that is called when the page / window is about to be closed, in order to return back to the
		/// previously shown page / window. Subclasses can override this method and return TRUE in order to cancel the
		/// closing and the navigation process.
		/// </summary>
		/// <param name="viewState">Indicates the lifecycle state the View is about to reach.</param>
		/// <returns></returns>
		public virtual bool CancelNavigatingFrom(ViewState viewState)
		{
			return false;
		}

		/// <summary>
		/// Method that is called when a page / window is closed. Subclasses can override this method to be notified
		/// when the page / window they represent is closed.
		/// </summary>
		/// <param name="viewState">Indicates the lifecycle state the View is about to reach.</param>
		public virtual void OnNavigatedFrom(ViewState viewState)
		{
			ViewState = viewState;
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Helper method that checks if some actions needs to be invoked on the main thread, and calls the
		/// dispatcher to do so.
		/// </summary>
		/// <param name="action">Action to be invoked using the dispatcher if necessary</param>
		protected void RunOnMainThread(Action action)
		{
			if (DispatcherHelper != null && !DispatcherHelper.IsRunningOnMainThread())
				DispatcherHelper.RunOnMainThread(action);
			else
				action.Invoke();
		}

		/// <summary>
		/// Loops through all Commands defined within the current Viewmodel and checks if methods exists
		/// that these Commands can be mapped to.
		/// </summary>
		private void WireCommandAutobinding()
		{
			foreach (PropertyInfo command in GetType().GetTypeInfo().DeclaredProperties
				.Where(p => p.PropertyType == typeof(BaseCommand)))
			{
				string methodName = command.Name.Replace("Command", String.Empty).Replace("command", String.Empty);
				string conditionName = String.Format("Can{0}", methodName);

				// Find a method with the Command's name
				MethodInfo method = GetType().GetTypeInfo().GetDeclaredMethod(methodName);
				if (method != null)
				{
					var returnType = method.ReturnType;
                    if (returnType == typeof(void) || 
						returnType == typeof(Task))
					{
						Expression<Func<bool>> condition = null;
						Expression<Func<object, bool>> conditionWithParam = null;
						bool hasParameters = false;
						bool isAsync = (returnType == typeof(Task));

						if (!method.GetParameters().Any())
						{
							// If available, retrieve the CanExecute condition
							PropertyInfo property = GetType().GetTypeInfo().GetDeclaredProperty(conditionName);
							if (property != null && property.PropertyType == typeof(Expression<Func<bool>>))
							{
								condition = (Expression<Func<bool>>)property.GetValue(this);
							}
						}
						else if (method.GetParameters().Count() == 1 && method.GetParameters().First().ParameterType == typeof(object))
						{
							hasParameters = true;

							// If available, retrieve the CanExecute condition
							PropertyInfo property = GetType().GetTypeInfo().GetDeclaredProperty(conditionName);
							if (property != null && property.PropertyType == typeof(Expression<Func<bool>>))
							{
								conditionWithParam = (Expression<Func<object, bool>>)property.GetValue(null);
							}
						}

						// Instantiate the Command
						if (!isAsync)
						{
							if (!hasParameters)
							{
								if (condition == null)
									command.SetValue(this, new BaseCommand((Action)method.CreateDelegate(typeof(Action), this)));
								else
									command.SetValue(this, new BaseCommand((Action)method.CreateDelegate(typeof(Action), this), condition, this));
							}
							else
							{
								if (conditionWithParam == null)
									command.SetValue(this, new BaseCommand((Action<object>)method.CreateDelegate(typeof(Action<object>), this)));
								else
									command.SetValue(this, new BaseCommand((Action<object>)method.CreateDelegate(typeof(Action<object>), this), conditionWithParam, this));
							}
						}
						else
						{
							if (!hasParameters)
							{
								if (condition == null)
									command.SetValue(this, new BaseCommand((Func<Task>)method.CreateDelegate(typeof(Func<Task>), this)));
								else
									command.SetValue(this, new BaseCommand((Func<Task>)method.CreateDelegate(typeof(Func<Task>), this), condition, this));
							}
							else
							{
								if (conditionWithParam == null)
									command.SetValue(this, new BaseCommand((Func<object, Task>)method.CreateDelegate(typeof(Func<object, Task>), this)));
								else
									command.SetValue(this, new BaseCommand((Func<object, Task>)method.CreateDelegate(typeof(Func<object, Task>), this), conditionWithParam, this));
							}
						}
					}
				}
			}
		}

		#endregion

		#region PropertyChanged event

		public new event PropertyChangedEventHandler PropertyChanged;

		protected new void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			if (PropertyChanged != null)
				RunOnMainThread(() => PropertyChanged(this, args));
		}

		protected new void NotifyPropertyChanged(string propertyName)
		{
			NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected new void NotifyPropertyChanged<T>(Expression<Func<T>> property)
		{
			string propertyName = ((MemberExpression)property.Body).Member.Name;
			NotifyPropertyChanged(propertyName);
		}

		#endregion

		#region Setter method

		/// <summary>
		/// Setter method that sets the private member field and fires the <c>PropertyChanged</c> event using the
		/// Dispatcher.
		/// </summary>
		protected new void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
		{
			Set(null, ref field, value, null, propertyName);
		}

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event using the
		/// Dispatcher, and calls a
		/// specified method to decide whether to continue updating the member field.
		/// </summary>
		protected new void Set<T>(Func<bool> before, ref T field, T value, [CallerMemberName] string propertyName = "")
		{
			Set(before, ref field, value, null, propertyName);
		}

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event using the
		/// Dispatcher, and calls a specified method after updating the member field.
		/// </summary>
		protected new void Set<T>(ref T field, T value, Action after, [CallerMemberName] string propertyName = "")
		{
			Set(null, ref field, value, after, propertyName);
		}

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event using the
		/// Dispatcher, and calls two specified methods: One before updating the member field to decide whether to
		/// continue, one after updating the member field.
		/// </summary>
		protected new void Set<T>(Func<bool> before, ref T field, T value, Action after, [CallerMemberName] string propertyName = "")
		{
			if (!ReferenceEquals(field, value))
			{
				// (1) if available, call 'before' method and decide whether to continue
				if (before != null)
				{
					bool continueMethod = before.Invoke();
					if (!continueMethod)
						return;
				}
				// (2) set field
				field = value;
				// (3) fire PropertyChanged, using the dispatcher if necessary
				NotifyPropertyChanged(propertyName);
				// (4) if available, call additional method
				if (after != null)
					after.Invoke();
			}
		}

		#endregion

		#region Deprecated setter methods

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event and optionally
		/// fires one or multiple commands' <c>CanExecuteChanged</c> events.
		/// </summary>
		[Obsolete("Use method Set<T>(ref T field, T value) instead")]
		protected new void Set<T>(ref T field, T value, Expression<Func<T>> property, params BaseCommand[] commands)
		{
			Set(ref field, value, property, null, commands);
		}

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event, calls a specified
		/// method after updating the member field, and optionally fires one or multiple commands' 
		/// <c>CanExecuteChanged</c> events.
		/// </summary>
		[Obsolete("Use method Set<T>(ref T field, T value, Action after) instead")]
		protected new void Set<T>(ref T field, T value, Expression<Func<T>> property, Action method,
			params BaseCommand[] commands)
		{
			if (!ReferenceEquals(field, value))
			{
				// (1) set field
				field = value;
				// (2) fire PropertyChanged
				NotifyPropertyChanged(property);
				// (3) for each command (if any), fire CanExecuteChanged
				if (commands != null)
					foreach (var command in commands)
						if (command != null)
							command.NotifyCanExecuteChanged();
				// (4) if available, call additional method
				if (method != null)
					method.Invoke();
			}
		}

		#endregion
	}
}
