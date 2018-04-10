/*
 * (c) 2014-2015 Andreas Kuntner
 */

using System.Windows;
using System.Windows.Input;

namespace MVVMbasics.Helpers
{
	/// <summary>
	/// Helper class that provides two attached properties which can be used in combination with the
	/// <see cref="MVVMbasics.Views.BaseView">EventToCommand</see> event handler and represent the Command that
	/// shall be invoked when the event is fired and its parameter.
	/// </summary>
	public class Event : DependencyObject
	{
		/// <summary>
		/// Property to be used in combination with the <see cref="MVVMbasics.Views.BaseView">EventToCommand</see>
		/// event handler. Represents the Command that shall be invoked when the event is fired.
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
			"Command",
			typeof(ICommand),
			typeof(Event),
			new PropertyMetadata(default(ICommand)));

		public static ICommand GetCommand(UIElement element)
		{
			return (ICommand)element.GetValue(CommandProperty);
		}

		public static void SetCommand(UIElement element, ICommand value)
		{
			element.SetValue(CommandProperty, value);
		}

		/// <summary>
		/// Property to be used in combination with the <see cref="MVVMbasics.Views.BaseView">EventToCommand</see>
		/// event handler. Represents the parameter to be passed to the Command.
		/// </summary>
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
			"CommandParameter", 
			typeof (object), 
			typeof (Event), 
			new PropertyMetadata(default(object)));

		public static object GetCommandParameter(UIElement element)
		{
			return element.GetValue(CommandParameterProperty);
		}

		public static void SetCommandParameter(UIElement element, object value)
		{
			element.SetValue(CommandParameterProperty, value);
		}
	}
}
