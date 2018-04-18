/*
 * (c) 2014-2018 Andreas Kuntner
 */

using System.Windows.Input;
using Xamarin.Forms;

namespace MVVMbasics.Helpers
{
	/// <summary>
	/// Helper class that provides two attached properties which can be used in combination with the
	/// <see cref="MVVMbasics.Views.BaseView">EventToCommand</see> event handler and represent the Command that
	/// shall be invoked when the event is fired and its parameter.
	/// </summary>
	public class Event : BindableObject
	{
		/// <summary>
		/// Property to be used in combination with the <see cref="MVVMbasics.Views.BaseView">EventToCommand</see>
		/// event handler. Represents the Command that shall be invoked when the event is fired.
		/// </summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.CreateAttached(
			"Command",
			typeof(ICommand),
			typeof(Event),
			default(ICommand)
		);

		public static ICommand GetCommand(VisualElement element)
		{
			return (ICommand)element.GetValue(CommandProperty);
		}

		public static void SetCommand(VisualElement element, ICommand value)
		{
			element.SetValue(CommandProperty, value);
		}

		/// <summary>
		/// Property to be used in combination with the <see cref="MVVMbasics.Views.BaseView">EventToCommand</see>
		/// event handler. Represents the parameter to be passed to the Command.
		/// </summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.CreateAttached(
			"CommandParameter",
			typeof(object),
			typeof(Event),
			default(object));

		public static object GetCommandParameter(VisualElement element)
		{
			return element.GetValue(CommandParameterProperty);
		}

		public static void SetCommandParameter(VisualElement element, object value)
		{
			element.SetValue(CommandParameterProperty, value);
		}
	}
}
