/*
 * (c) 2013-2018 Andreas Kuntner
 */
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MVVMbasics.Commands;

namespace MVVMbasics.Models
{
	/// <summary>
	/// Basic data model, not to be implemented directly, but to be inherited by the actual data models. Contains
	/// convenience functions that allow the implementation of <c>INotifyPropertyChanged</c> and some command's
	/// <c>CanExecuteChanged</c> events in code snippets as short as possible.
	/// </summary>
	public abstract class BaseModel : INotifyPropertyChanged
	{
		#region Constructor

		/// <summary>
		/// Empty constructor.
		/// </summary>
		public BaseModel()
		{
			// Does nothing, necessary only for serialization.
		}

		#endregion

		#region PropertyChanged event

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, args);
		}

		protected void NotifyPropertyChanged(string propertyName)
		{
			NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected void NotifyPropertyChanged<T>(Expression<Func<T>> property)
		{
			string propertyName = ((MemberExpression)property.Body).Member.Name;
			NotifyPropertyChanged(propertyName);
		}

		#endregion

		#region Setter method

	    /// <summary>
		/// Setter method that sets the private member field and fires the <c>PropertyChanged</c> event.
	    /// </summary>
	    protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
	    {
            Set(null, ref field, value, null, propertyName);
	    }

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event and calls a
		/// specified method to decide whether to continue updating the member field.
		/// </summary>
		protected void Set<T>(Func<bool> before, ref T field, T value, [CallerMemberName] string propertyName = "")
		{
			Set(before, ref field, value, null, propertyName);
		}

		/// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event  and calls a
		/// specified method after updating the member field.
		/// </summary>
		protected void Set<T>(ref T field, T value, Action after, [CallerMemberName] string propertyName = "")
		{
			Set(null, ref field, value, after, propertyName);
		}

	    /// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event and calls two
		/// specified methods: One before updating the member field to decide whether to continue, one after updating
		/// the member field.
		/// </summary>
		protected void Set<T>(Func<bool> before, ref T field, T value, Action after, [CallerMemberName] string propertyName = "")
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
				// (3) fire PropertyChanged
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
	    protected void Set<T>(ref T field, T value, Expression<Func<T>> property, params BaseCommand[] commands)
	    {
            Set(ref field, value, property, null, commands);
	    }

	    /// <summary>
		/// Setter method that sets the private member field, fires the <c>PropertyChanged</c> event, calls a specified
		/// method after updating the member field, and optionally fires one or multiple commands' 
		/// <c>CanExecuteChanged</c> events.
		/// </summary>
		[Obsolete("Use method Set<T>(ref T field, T value, Action after) instead")]
		protected void Set<T>(ref T field, T value, Expression<Func<T>> property, Action method, 
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

		#region Command generation convenience methods

		/// <summary>
		/// Creates a Command and registers a parameterless method to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		/// <returns></returns>
		protected BaseCommand CreateCommand(Action execute)
		{
			return new BaseCommand(execute);
		}

		/// <summary>
		/// Creates a Command and registers a method with one parameter of type <c>object</c> to it.
		/// </summary>
		/// <param name="execute">Method with one parameter of type <c>object</c>.</param>
		/// <returns></returns>
		protected BaseCommand CreateCommand(Action<object> execute)
		{
			return new BaseCommand(execute);
		}

		/// <summary>
		/// Creates a Command with a <c>CanExecute</c> condition and registers a parameterless method to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		/// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
		/// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
		/// <returns></returns>
		protected BaseCommand CreateCommand(Action execute, Expression<Func<bool>> canExecute, 
			params Expression<Func<object>>[] dependsOnProperties)
		{
			return new BaseCommand(execute, canExecute, this, dependsOnProperties);
		}

		/// <summary>
		/// Creates a Command with a <c>CanExecute</c> condition and registers a method with one parameter of type
		/// <c>object</c> to it.
		/// </summary>
		/// <param name="execute">Method with one parameter of type <c>object</c>.</param>
		/// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
		/// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
		/// <returns></returns>
		protected BaseCommand CreateCommand(Action<object> execute, Expression<Func<object, bool>> canExecute, 
			params Expression<Func<object>>[] dependsOnProperties)
		{
			return new BaseCommand(execute, canExecute, this, dependsOnProperties);
		}

		/// <summary>
		/// Creates an asynchronous Command and registers a parameterless method to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		/// <returns></returns>
		protected BaseCommand CreateAsyncCommand(Func<Task> execute)
		{
			return new BaseCommand(execute);
		}

        /// <summary>
        /// Creates an asynchronous Command and registers a method with one parameter of type <c>object</c> to it.
        /// </summary>
        /// <param name="execute">Method with one parameter of type <c>object</c>.</param>
        /// <returns></returns>
        protected BaseCommand CreateAsyncCommand(Func<object, Task> execute)
		{
			return new BaseCommand(execute);
		}

        /// <summary>
        /// Creates an asynchronous Command with a <c>CanExecute</c> condition and registers a parameterless method to it.
        /// </summary>
        /// <param name="execute">Parameterless method.</param>
        /// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
        /// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
        /// <returns></returns>
        protected BaseCommand CreateAsyncCommand(Func<Task> execute, Expression<Func<bool>> canExecute, 
			params Expression<Func<object>>[] dependsOnProperties)
		{
			return new BaseCommand(execute, canExecute, this, dependsOnProperties);
		}

        /// <summary>
        /// Creates an asynchronous Command with a <c>CanExecute</c> condition and registers a method with one parameter of type
        /// <c>object</c> to it.
        /// </summary>
        /// <param name="execute">Method with one parameter of type <c>object</c>.</param>
        /// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
        /// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
        /// <returns></returns>
        protected BaseCommand CreateAsyncCommand(Func<object, Task> execute, Expression<Func<object, bool>> canExecute, 
			params Expression<Func<object>>[] dependsOnProperties)
		{
			return new BaseCommand(execute, canExecute, this, dependsOnProperties);
		}

		#endregion
	}
}
