/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;

namespace MVVMbasics.Services
{
	public delegate void TickHandler(Guid id);

	/// <summary>
	/// Interface specifying all methods for a timer service.
	/// </summary>
	public interface ITimerService : IService
	{
		Guid StartOnce(TimeSpan interval, Action callback);
		Guid StartLooping(TimeSpan interval, Action callback);
		void Stop(params Guid[] ids);
	}
}
