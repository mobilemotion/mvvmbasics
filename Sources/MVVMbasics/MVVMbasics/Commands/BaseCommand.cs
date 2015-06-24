/*
 * (c) 2013-2015 Andreas Kuntner
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;

namespace MVVMbasics.Commands
{
	/// <summary>
	/// Simple delegate command all actual commands can be derived from. Works with and without parameters.
	/// </summary>
	public class BaseCommand : ICommand
	{
		#region Members

		/// <summary>
		/// Method registered to this Command.
		/// </summary>
		private readonly Action<object> _execute;

		/// <summary>
		/// Condition that defines whether this Command is enabled or not.
		/// </summary>
		private readonly Func<object, bool> _canExecute = null;

		/// <summary>
		/// List that hold the names of all Properties this Command depends on. If one of these Properties
		/// changes, the Command's <c>CanExecute</c> condition needs to be re-evaluated.
		/// </summary>
		private readonly List<string> _dependsOnProperties = new List<string>();

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
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			// Store the method and, if applicable, the CanExecute condition
			_execute = o => execute();

			if (canExecute != null)
			{
				Func<bool> canExecuteFunc = canExecute.Compile();
				_canExecute = o => canExecuteFunc();

				// Reset the list of Properties this Command depends on
				_dependsOnProperties.Clear();

				// Fill the list of Properties this Command depends on
				if (dependsOnProperties != null && dependsOnProperties.Any())
				{
					// If Properties have been specified as parameters, simply store their names
					StoreProperties(dependsOnProperties);
				}
				else
				{
					// If Properties have been specified as parameters, parse the CanExecute condition to find all
					// Properties it depends on
					ParseExpresionTree(canExecute, _dependsOnProperties);
				}

				// Register to the host's PropertyChanged event, to be notified whenever a Property this Command depends
				// on changes
				if (owner != null && _dependsOnProperties.Any())
				{
					owner.PropertyChanged += OwnerPropertyChanged;
				}
			}
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
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			// Store the method and, if applicable, the CanExecute condition
			_execute = execute;

			if (canExecute != null)
			{
				Func<object, bool> canExecuteFunc = canExecute.Compile();
				_canExecute = canExecuteFunc;

				// Reset the list of Properties this Command depends on
				_dependsOnProperties.Clear();

				// Fill the list of Properties this Command depends on
				if (dependsOnProperties != null && dependsOnProperties.Any())
				{
					// If Properties have been specified as parameters, simply store their names
					StoreProperties(dependsOnProperties);
				}
				else
				{
					// If Properties have been specified as parameters, parse the CanExecute condition to find all
					// Properties it depends on
					ParseExpresionTree(canExecute, _dependsOnProperties);
				}

				// Register to the host's PropertyChanged event, to be notified whenever a Property this Command depends
				// on changes
				if (owner != null && _dependsOnProperties.Any())
				{
					owner.PropertyChanged += OwnerPropertyChanged;
				}
			}
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
		public void Execute(object parameter)
		{
			if (CanExecute(parameter))
				_execute(parameter);
		}

		#endregion

		#region Private helper methods

		/// <summary>
		/// Event that is fired whenever one of the host's bindable Properties changes. Checks if the
		/// respective Property is included in the list of Properties this Commands depends on, in which
		/// case the <c>CanExecute</c> condition needs to be re-evaluated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_dependsOnProperties.Contains(e.PropertyName))
				NotifyCanExecuteChanged();
		}

		/// <summary>
		/// Loops through all Properties this Command depends on and stores their names in a list.
		/// </summary>
		/// <param name="dependsOnProperties">Properties this Command depends on</param>
		private void StoreProperties(Expression<Func<object>>[] dependsOnProperties)
		{
			foreach (var dependsOnProperty in dependsOnProperties)
			{
				string propertyName = null;
				var expression = dependsOnProperty.Body;
				if (expression is MemberExpression)
					propertyName = ((MemberExpression)expression).Member.Name;
				else if (expression is UnaryExpression)
					propertyName = ((MemberExpression)((UnaryExpression)expression).Operand).Member.Name;
				if (!String.IsNullOrEmpty(propertyName))
					_dependsOnProperties.Add(propertyName);
			}
		}

		/// <summary>
		/// Parses an expression (in pratice, a Command's <c>CanExecute</c> condition) to find all Properties
		/// it references. Depending on the expression's type, this method is called recursively for all sub-
		/// expressions. If a reference to a Property is found, the Property's name is added to a list.
		/// </summary>
		/// <param name="inputExpression">Expression to be parsed</param>
		/// <param name="properties">List of detected Property names</param>
		private void ParseExpresionTree(Expression inputExpression, List<string> properties)
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
					PropertyInfo property = member as PropertyInfo;
					Type owner = property.DeclaringType;
					if (owner.GetTypeInfo().ImplementedInterfaces.Contains(typeof(INotifyPropertyChanged)))
					{
						string propertyName = property.Name;
						if (!properties.Contains(propertyName))
							properties.Add(propertyName);
					}
				}
				ParseExpresionTree(e.Expression, properties);
			}

			// For all other types, just check their child expressions
			else if (inputExpression is BlockExpression)
			{
				var e = inputExpression as BlockExpression;
				foreach (var expression in e.Expressions)
				{
					ParseExpresionTree(expression, properties);
				}
			}
			else if (inputExpression is BinaryExpression)
			{
				var e = inputExpression as BinaryExpression;
				ParseExpresionTree(e.Left, properties);
				ParseExpresionTree(e.Right, properties);
			}
			else if (inputExpression is ConditionalExpression)
			{
				var e = inputExpression as ConditionalExpression;
				ParseExpresionTree(e.Test, properties);
				ParseExpresionTree(e.IfTrue, properties);
				ParseExpresionTree(e.IfFalse, properties);
			}
			else if (inputExpression is InvocationExpression)
			{
				var e = inputExpression as InvocationExpression;
				ParseExpresionTree(e.Expression, properties);
			}
			else if (inputExpression is LambdaExpression)
			{
				var e = inputExpression as LambdaExpression;
				ParseExpresionTree(e.Body, properties);
			}
			else if (inputExpression is ListInitExpression)
			{
				var e = inputExpression as ListInitExpression;
				ParseExpresionTree(e.NewExpression, properties);
			}
			else if (inputExpression is LoopExpression)
			{
				var e = inputExpression as LoopExpression;
				ParseExpresionTree(e.Body, properties);
			}
			else if (inputExpression is MemberInitExpression)
			{
				var e = inputExpression as MemberInitExpression;
				ParseExpresionTree(e.NewExpression, properties);
			}
			else if (inputExpression is NewExpression)
			{
				var e = inputExpression as NewExpression;
				foreach (var expression in e.Arguments)
				{
					ParseExpresionTree(expression, properties);
				}
			}
			else if (inputExpression is NewArrayExpression)
			{
				var e = inputExpression as NewArrayExpression;
				foreach (var expression in e.Expressions)
				{
					ParseExpresionTree(expression, properties);
				}
			}
			else if (inputExpression is SwitchExpression)
			{
				var e = inputExpression as SwitchExpression;
				ParseExpresionTree(e.SwitchValue, properties);
				ParseExpresionTree(e.DefaultBody, properties);
				foreach (var c in e.Cases)
				{
					ParseExpresionTree(c.Body, properties);
					foreach (var expression in c.TestValues)
					{
						ParseExpresionTree(expression, properties);
					}
				}
			}
			else if (inputExpression is TypeBinaryExpression)
			{
				var e = inputExpression as TypeBinaryExpression;
				ParseExpresionTree(e.Expression, properties);
			}
			else if (inputExpression is TryExpression)
			{
				var e = inputExpression as TryExpression;
				ParseExpresionTree(e.Body, properties);
				ParseExpresionTree(e.Fault, properties);
				ParseExpresionTree(e.Finally, properties);
			}
			else if (inputExpression is UnaryExpression)
			{
				var e = inputExpression as UnaryExpression;
				ParseExpresionTree(e.Operand, properties);
			}

			// Expressions of type MethodCallExpression are a special case: Only analyze their arguments and their owner, not their body
			else if (inputExpression is MethodCallExpression)
			{
				var e = inputExpression as MethodCallExpression;
				foreach (Expression argument in e.Arguments)
				{
					ParseExpresionTree(argument, properties);
				}
				if (e.Object != null)
				{
					ParseExpresionTree(e.Object, properties);
				}
			}

			// The following type(s) of expressions need not to be handled:
			// - ConstantExpression (beacuse Constant values need no binding since they won't change)
		}

		#endregion
	}
}