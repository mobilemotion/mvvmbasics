/*
 * (c) 2013-2015 Andreas Kuntner
 */

using MVVMbasics.Attributes;
using Xamarin.Forms;

namespace MVVMbasics.Services
{
	/// <summary>
	/// Service which allows showing alerts on the user interface through message box popups.
	/// </summary>
	[MvvmService]
	public class MessageboxService : IMessageboxService
	{
		/// <summary>
		/// Shows a message box containing a specified message.
		/// </summary>
		/// <param name="message">Message to be displayed.</param>
		public void Show(string message)
		{
			var app = Application.Current as BaseApplication;
			if (app != null)
				app.MainPage.DisplayAlert ("", message, "OK");
		}

		/// <summary>
		/// Shows a message box containing a specified message and a specified title.
		/// </summary>
		/// <param name="message">Message to be displayed.</param>
		/// <param name="title">Title to be displayed.</param>
		public void Show(string message, string title)
		{
			var app = Application.Current as BaseApplication;
			if (app != null)
				app.MainPage.DisplayAlert (title, message, "OK");
		}
	}
}
