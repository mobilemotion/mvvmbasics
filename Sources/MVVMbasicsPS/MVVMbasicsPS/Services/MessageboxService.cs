/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System.Windows;
using MVVMbasics.Attributes;

namespace MVVMbasics.Services
{
	/// <summary>
	/// Service which allows showing alerts to the user through message box popups.
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
			MessageBox.Show(message);
		}

		/// <summary>
		/// Shows a message box containing a specified message and a specified title.
		/// </summary>
		/// <param name="message">Message to be displayed.</param>
		/// <param name="title">Title to be displayed.</param>
		public void Show(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButton.OK);
		}
	}
}
