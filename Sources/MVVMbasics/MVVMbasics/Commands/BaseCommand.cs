/*
 * (c) 2013-2018 Andreas Kuntner
 * 
 * some corrections Frank Albert
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using MVVMbasics.Exceptions;
using System.Threading.Tasks;

namespace MVVMbasics.Commands
{
	/// <summary>
	/// Basic implementation of an MVVM command.
	/// </summary>
	public class BaseCommand : ICommand
	{
		#region Members

		/// <summary>
		/// Method registered to this Command.
		/// </summary>
		private readonly Action<object> _execute;

		/// <summary>
		/// Asynchronous method registered to this Command.
		/// </summary>
		private readonly Func<object, Task> _executeAsync;

		/// <summary>
		/// Condition that defines whether this Command is enabled or not.
		/// </summary>
		private readonly Func<object, bool> _canExecute = null;

		/// <summary>
		/// Reference to the Viewmodel holding this Command instance.
		/// </summary>
		private readonly INotifyPropertyChanged _viewmodel;

		/// <summary>
		/// List that hold the names of all Properties this Command depends on. If one of these Properties
		/// changes, the Command's <c>CanExecute</c> condition needs to be re-evaluated.
		/// </summary>
		private readonly Dictionary<Type, List<string>> _dependsOnProperties = new Dictionary<Type, List<string>>();

		#endregion

		#region CanExecuteChanged event

		/// <summary>
		/// Event indicating that the <c>CanExecute</c> condition might have changed and must be re-evaluated.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Fires the <see cref="CanExecuteChanged">CanExecuteChanged</see> event, forcing the <c>CanExecute</c>
		/// condition to be re-evaluated.
		/// </summary>
		public void NotifyCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Generic constructor for a parameterless command that may be used by derived classes.
		/// </summary>
		protected BaseCommand(Expression<Func<bool>> canExecute, INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties)
		{
			if (canExecute != null)
			{
				if (owner == null)
					throw new ArgumentNullException("owner");

				Func<bool> canExecuteFunc = canExecute.Compile();
				_canExecute = o => canExecuteFunc();

				// Reset the list of Properties this Command depends on
				_dependsOnProperties.Clear();

				// Store a reference to the Viewmodel
				_viewmodel = owner;

				// Fill the list of Properties this Command depends on
				if (dependsOnProperties != null && dependsOnProperties.Any())
				{
					// If Properties have been specified as parameters, simply store their names
					StoreProperties(dependsOnProperties);
				}
				else
				{
					// If no Properties have been specified as parameters, parse the CanExecute condition to find all
					// Properties it depends on
					ParseExpresionTree(canExecute);
				}
			}
		}

		/// <summary>
		/// Generic constructor for a command with one parameter of type <c>object</c> that may be used by derived
		/// classes.
		/// </summary>
		protected BaseCommand(Expression<Func<object, bool>> canExecute, INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties)
		{
			if (canExecute != null)
			{
				if (owner == null)
					throw new ArgumentNullException("owner");

				Func<object, bool> canExecuteFunc = canExecute.Compile();
				_canExecute = canExecuteFunc;

				// Reset the list of Properties this Command depends on
				_dependsOnProperties.Clear();

				// Store a reference to the Viewmodel
				_viewmodel = owner;

				// Fill the list of Properties this Command depends on
				if (dependsOnProperties != null && dependsOnProperties.Any())
				{
					// If Properties have been specified as parameters, simply store their names
					StoreProperties(dependsOnProperties);
				}
				else
				{
					// If no Properties have been specified as parameters, parse the CanExecute condition to find all
					// Properties it depends on
					ParseExpresionTree(canExecute);
				}
			}
		}

		/// <summary>
		/// Constructor that creates a Command and registers a parameterless method to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		public BaseCommand(Action execute)
			: this(o => execute(), null, null)
		{
		}

		/// <summary>
		/// Constructor that creates a Command and registers a method with one parameter of type <c>object</c> to it.
		/// </summary>
		/// <param name="execute">Method with one parameter of type <c>object</c>.</param>
		public BaseCommand(Action<object> execute)
			: this(execute, null, null)
		{
		}

		/// <summary>
		/// Constructor that creates a Command with a <c>CanExecute</c> condition and registers a parameterless method
		/// to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		/// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
		/// <param name="owner">Model or Viewmodel that hosts this Command (and all Properties it depends on).</param>
		/// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
		public BaseCommand(Action execute, Expression<Func<bool>> canExecute, INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties)
			: this(canExecute, owner, dependsOnProperties)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = o => execute();
			_executeAsync = null;
		}

		/// <summary>
		/// Constructor that creates a Command with a <c>CanExecute</c> condition and registers a method with one
		/// parameter of type <c>object</c> to it.
		/// </summary>
		/// <param name="execute">Method with one parameter of type <c>object</c>.</param>
		/// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
		/// <param name="owner">Model or Viewmodel that hosts this Command (and all Properties it depends on).</param>
		/// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
		public BaseCommand(Action<object> execute, Expression<Func<object, bool>> canExecute, INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties)
			: this(canExecute, owner, dependsOnProperties)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_executeAsync = null;
		}

		/// <summary>
		/// Constructor that creates a Command and registers a parameterless method to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		public BaseCommand(Func<Task> execute)
			: this(o => execute(), null, null)
		{
		}

		/// <summary>
		/// Constructor that creates a Command and registers a method with one parameter of type <c>object</c> to it.
		/// </summary>
		/// <param name="execute">Method with one parameter of type <c>object</c>.</param>
		public BaseCommand(Func<object, Task> execute)
			: this(execute, null, null)
		{
		}

		/// <summary>
		/// Constructor that creates a Command with a <c>CanExecute</c> condition and registers a parameterless method
		/// to it.
		/// </summary>
		/// <param name="execute">Parameterless method.</param>
		/// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
		/// <param name="owner">Model or Viewmodel that hosts this Command (and all Properties it depends on).</param>
		/// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
		public BaseCommand(Func<Task> execute, Expression<Func<bool>> canExecute, INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties)
			: this(canExecute, owner, dependsOnProperties)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_executeAsync = o => execute();
			_execute = null;
		}

		/// <summary>
		/// Constructor that creates a Command with a <c>CanExecute</c> condition and registers a method with one
		/// parameter of type <c>object</c> to it.
		/// </summary>
		/// <param name="execute">Method with one parameter of type <c>object</c>.</param>
		/// <param name="canExecute">Condition that defines whether this Command is enabled or not.</param>
		/// <param name="owner">Model or Viewmodel that hosts this Command (and all Properties it depends on).</param>
		/// <param name="dependsOnProperties">List of Properties this Command depends on.</param>
		public BaseCommand(Func<object, Task> execute, Expression<Func<object, bool>> canExecute, INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties)
			: this(canExecute, owner, dependsOnProperties)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_executeAsync = execute;
			_execute = null;
		}

		#endregion

		#region Deprecated constructors

		[Obsolete(@"Use Constructor BaseCommand(Action execute, Expression<Func<bool>> canExecute, INotifyPropertyChanged
			owner, params Expression<Func<object>>[] dependsOnProperties) or Model's method CreateCommand(Action execute,
			Expression<Func<bool>> canExecute, params Expression<Func<object>>[] dependsOnProperties) instead")]
		public BaseCommand(Action execute, Func<bool> canExecute)
			: this(o => execute(), o => canExecute())
		{
		}

		[Obsolete(@"Use Constructor BaseCommand(Action execute, Expression<Func<object, bool>> canExecute,
			INotifyPropertyChanged owner, params Expression<Func<object>>[] dependsOnProperties) or Model's method
			CreateCommand(Action execute, Expression<Func<object, bool>> canExecute, params Expression<Func<object>>[]
			dependsOnProperties) instead")]
		public BaseCommand(Action<object> execute, Predicate<object> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = new Func<object, bool>(canExecute);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Evaluates the <c>CanExecute</c> condition.
		/// </summary>
		/// <returns>TRUE if the Command shall be enabled, FALSE otherwise.</returns>
		public bool CanExecute()
		{
			return _canExecute == null || _canExecute(null);
		}

		/// <summary>
		/// Evaluates the <c>CanExecute</c> condition.
		/// </summary>
		/// <param name="parameter">Parameter to be passed to the <c>CanExecute</c> condition.</param>
		/// <returns>TRUE if the Command shall be enabled, FALSE otherwise.</returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute(parameter);
		}

		/// <summary>
		/// Calls the method that is registered to this Command.
		/// </summary>
		public void Execute()
		{
			Execute(null);
		}

		/// <summary>
		/// Calls the method that is registered to this Command.
		/// </summary>
		/// <param name="parameter">Parameter to be passed to the method.</param>
		public async void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				if (_execute != null)
					_execute(parameter);
				else if (_executeAsync != null)
					await _executeAsync(parameter);
			}
		}

		/// <summary>
		/// Calls the asynchronous method that is registered to this Command.
		/// </summary>
		/// <param name="parameter">Parameter to be passed to the method.</param>
		/// <returns>Task representing the asynchronous operation.</returns>
		public Task ExecuteAsync(object parameter)
		{
			if (_executeAsync != null && CanExecute(parameter))
				return _executeAsync(parameter);
			return null;
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Event that is fired whenever one of the host's bindable Properties changes. Checks if the
		/// respective Property is included in the list of Properties this Commands depends on, in which
		/// case the <c>CanExecute</c> condition needs to be re-evaluated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_dependsOnProperties.ContainsKey(sender.GetType()) &&
				_dependsOnProperties[sender.GetType()].Contains(e.PropertyName))
			{
				this.NotifyCanExecuteChanged();
			}
		}

		/// <summary>
		/// Loops through all Properties this Command depends on and stores their names in a list.
		/// </summary>
		/// <param name="dependsOnProperties">Properties this Command depends on</param>
		private void StoreProperties(Expression<Func<object>>[] dependsOnProperties)
		{
			foreach (var dependsOnProperty in dependsOnProperties)
			{
				// Retrieve the property's name and an expression that describes its containing class
				string propertyName = null;
				Expression ownerExpression = null;
				var expression = dependsOnProperty.Body;
				var memberExpression = expression as MemberExpression;
				if (memberExpression != null)
				{
					propertyName = memberExpression.Member.Name;
					ownerExpression = memberExpression.Expression;
				}
				else
				{
					var unaryExpression = expression as UnaryExpression;
					if (unaryExpression != null)
					{
						propertyName = ((MemberExpression)unaryExpression.Operand).Member.Name;
						ownerExpression = ((MemberExpression)unaryExpression.Operand).Expression;
					}
				}

				// Store the detected property and subscribe to its container's PropertyChanged event
				if (!String.IsNullOrEmpty(propertyName) && ownerExpression != null)
					StoreProperty(propertyName, ownerExpression);
			}
		}

		/// <summary>
		/// For a single property, retrieved its containing class, stores the property and the container's
		/// type in a global list, and subscribes to the container's <code>PropertyChanged</code> event.
		/// </summary>
		/// <param name="propertyName">Property to be stored</param>
		/// <param name="inputExpression">The property's containing class</param>
		private void StoreProperty(string propertyName, Expression inputExpression)
		{
			if (_viewmodel != null)
			{
				// Usually, properties are defined directly within the Viewmodel class
				INotifyPropertyChanged owner = _viewmodel;
				var e = inputExpression as MemberExpression;
				if (e != null)
				{
					// If a property is declared within a container class, find out if this container
					// is declared as Field or Property within the Viewmodel class, and retrieved its
					// actual instance
					var property = e.Member as PropertyInfo;
					if (property != null)
					{
						object ownerValue;
						if (property.DeclaringType != _viewmodel.GetType())
						{
							try
							{
								// In case of a singeton instance call, we can retrieve the value in spite of
								// nested container declarations
								ownerValue = property.GetValue(e);
							}
							catch (Exception)
							{
								// In case of nested container types, the actual instance can not be retrieved.
								// In this case, throw an exception.
								throw new ExpressionResolutionException(String.Format("The container class of the property {0} (expression: {1}) could not be resolved", propertyName, e.ToString()));
							}
						}
						else
						{
							ownerValue = property.GetValue(_viewmodel);
						}
						owner = ownerValue as INotifyPropertyChanged;
					}
					else
					{
						var field = e.Member as FieldInfo;
						if (field != null)
						{
							if (field.DeclaringType != _viewmodel.GetType())
							{
								// In case of nested container types, the actual instance can not be retrieved.
								// In this case, throw an exception.
								throw new ExpressionResolutionException(String.Format("The container class of the property {0} (expression: {1}) could not be resolved", propertyName, e.ToString()));
							}
							var ownerValue = field.GetValue(_viewmodel);
							owner = ownerValue as INotifyPropertyChanged;
						}
					}
				}
				if (inputExpression != null && owner != null)
				{
					// Store the container class' type and the property's name in a global list, and (if not
					// done already) subscribe to the container's PropertyChanged event
					var ownerType = owner.GetType();
					if (_dependsOnProperties.ContainsKey(ownerType))
					{
						if (!_dependsOnProperties[ownerType].Contains(propertyName))
							_dependsOnProperties[ownerType].Add(propertyName);
					}
					else
					{
						_dependsOnProperties.Add(ownerType, new List<string> { propertyName });
						owner.PropertyChanged += OwnerPropertyChanged;
					}
				}
			}
		}

		/// <summary>
		/// Parses an expression (in pratice, a Command's <c>CanExecute</c> condition) to find all Properties
		/// it references. Depending on the expression's type, this method is called recursively for all sub-
		/// expressions. If a reference to a Property is found, the Property's name and its declaring class
		/// are stored using the <code>StoreProperty</code> method.
		/// </summary>
		/// <param name="inputExpression">Expression to be parsed</param>
		private void ParseExpresionTree(Expression inputExpression)
		{
			// When a MemberExpression is found, check if the actual member is a Property of a class that
			// implements INotifyPropertyChanged, and in that case store it. In addition, recursively check
			// the member's owner expression.
			if (inputExpression is MemberExpression)
			{
				var e = inputExpression as MemberExpression;
				var member = e.Member;
				if (member is PropertyInfo)
				{
					var property = member as PropertyInfo;
					var ownerType = property.DeclaringType;
					if (ownerType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(INotifyPropertyChanged)))
					{
						StoreProperty(property.Name, e.Expression);
					}
				}
				ParseExpresionTree(e.Expression);
			}

			// For all other types, just check their child expressions until a MemberExpression is found
			else if (inputExpression is BlockExpression)
			{
				var e = inputExpression as BlockExpression;
				foreach (var expression in e.Expressions)
				{
					ParseExpresionTree(expression);
				}
			}
			else if (inputExpression is BinaryExpression)
			{
				var e = inputExpression as BinaryExpression;
				ParseExpresionTree(e.Left);
				ParseExpresionTree(e.Right);
			}
			else if (inputExpression is ConditionalExpression)
			{
				var e = inputExpression as ConditionalExpression;
				ParseExpresionTree(e.Test);
				ParseExpresionTree(e.IfTrue);
				ParseExpresionTree(e.IfFalse);
			}
			else if (inputExpression is InvocationExpression)
			{
				var e = inputExpression as InvocationExpression;
				ParseExpresionTree(e.Expression);
			}
			else if (inputExpression is LambdaExpression)
			{
				var e = inputExpression as LambdaExpression;
				ParseExpresionTree(e.Body);
			}
			else if (inputExpression is ListInitExpression)
			{
				var e = inputExpression as ListInitExpression;
				ParseExpresionTree(e.NewExpression);
			}
			else if (inputExpression is LoopExpression)
			{
				var e = inputExpression as LoopExpression;
				ParseExpresionTree(e.Body);
			}
			else if (inputExpression is MemberInitExpression)
			{
				var e = inputExpression as MemberInitExpression;
				ParseExpresionTree(e.NewExpression);
			}
			else if (inputExpression is NewExpression)
			{
				var e = inputExpression as NewExpression;
				foreach (var expression in e.Arguments)
				{
					ParseExpresionTree(expression);
				}
			}
			else if (inputExpression is NewArrayExpression)
			{
				var e = inputExpression as NewArrayExpression;
				foreach (var expression in e.Expressions)
				{
					ParseExpresionTree(expression);
				}
			}
			else if (inputExpression is SwitchExpression)
			{
				var e = inputExpression as SwitchExpression;
				ParseExpresionTree(e.SwitchValue);
				ParseExpresionTree(e.DefaultBody);
				foreach (var c in e.Cases)
				{
					ParseExpresionTree(c.Body);
					foreach (var expression in c.TestValues)
					{
						ParseExpresionTree(expression);
					}
				}
			}
			else if (inputExpression is TypeBinaryExpression)
			{
				var e = inputExpression as TypeBinaryExpression;
				ParseExpresionTree(e.Expression);
			}
			else if (inputExpression is TryExpression)
			{
				var e = inputExpression as TryExpression;
				ParseExpresionTree(e.Body);
				ParseExpresionTree(e.Fault);
				ParseExpresionTree(e.Finally);
			}
			else if (inputExpression is UnaryExpression)
			{
				var e = inputExpression as UnaryExpression;
				ParseExpresionTree(e.Operand);
			}

			// Expressions of type MethodCallExpression are a special case: Only analyze their arguments and
			// their owner, not their body
			else if (inputExpression is MethodCallExpression)
			{
				var e = inputExpression as MethodCallExpression;
				foreach (Expression argument in e.Arguments)
				{
					ParseExpresionTree(argument);
				}
				if (e.Object != null)
				{
					ParseExpresionTree(e.Object);
				}
			}

			// The following type(s) of expressions need not to be handled:
			// - ConstantExpression (because Constant values need no binding since they won't change)
		}

		#endregion
	}
}