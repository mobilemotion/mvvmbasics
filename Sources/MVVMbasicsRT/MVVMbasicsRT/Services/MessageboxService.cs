/*
 * (c) 2013-2015 Andreas Kuntner
 */

using Windows.UI.Popups;
using MVVMbasics.Attributes;

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
#pragma warning disable 4014
			new MessageDialog(message).ShowAsync();
#pragma warning restore 4014
		}

		/// <summary>
		/// Shows a message box containing a specified message and a specified title.
		/// </summary>
		/// <param name="message">Message to be displayed.</param>
		/// <param name="title">Title to be displayed.</param>
		public void Show(string message, string title)
		{
#pragma warning disable 4014
			new MessageDialog(message, title).ShowAsync();
#pragma warning restore 4014
		}
	}
}
