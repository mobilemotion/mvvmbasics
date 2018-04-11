/*
 * (c) 2013-2018 Andreas Kuntner
 */

namespace MVVMbasics.Services
{
	/// <summary>
	/// Interface specifying all methods for a service which allows to show alerts on the user interface through message
	/// box popups.
	/// </summary>
	public interface IMessageboxService : IService
	{
		void Show(string message);
		void Show(string message, string title);
	}
}
