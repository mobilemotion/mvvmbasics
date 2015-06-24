/*
 * (c) 2014-2015 Andreas Kuntner
 */

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using MVVMbasics.Attributes;

namespace MVVMbasics.Services
{
	/// <summary>
	/// Service which provides timer methods.
	/// </summary>
	[MvvmService]
	public class TimerService : ITimerService
	{
		/// <summary>
		/// Timer objects.
		/// </summary>
		private readonly Dictionary<Guid, DispatcherTimer> _timers = new Dictionary<Guid, DispatcherTimer>();

		/// <summary>
		/// Creates a new timer that will run once, and starts it.
		/// <param name="interval">Timer interval</param>
		/// <param name="callback">Method to be executed on the timer tick event.</param>
		/// </summary>
		public Guid StartOnce(TimeSpan interval, Action callback)
		{
			Guid id = Guid.NewGuid();
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = interval;
			timer.Tick += (sender, args) =>
			{
				callback.Invoke();
				timer.Stop();
				_timers.Remove(id);
			};
			_timers.Add(id, timer);
			timer.Start();
			return id;
		}

		/// <summary>
		/// Creates a new timer that will start over again, and starts it.
		/// <param name="interval">Timer interval</param>
		/// <param name="callback">Method to be executed on each timer tick event.</param>
		/// </summary>
		public Guid StartLooping(TimeSpan interval, Action callback)
		{
			Guid id = Guid.NewGuid();
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = interval;
			timer.Tick += (sender, args) => callback.Invoke();
			_timers.Add(id, timer);
			timer.Start();
			return id;
		}

		/// <summary>
		/// Stops some or all timers.
		/// <param name="ids">IDs of those timers that shall be stopped and removed. If not provided, all
		/// existing timers will be stopped and removed.</param>
		/// </summary>
		public void Stop(params Guid[] ids)
		{
			// If no parameters are passed, stop and remove all existing timers
			if (ids.Length == 0)
			{
				foreach (var timer in _timers)
				{
					timer.Value.Stop();
				}
				_timers.Clear();
			}
			// Else, if one or more timers are passed as parameters, stop and remove only these
			else
			{
				foreach (var guid in ids)
				{
					if (_timers.ContainsKey(guid))
					{
						_timers[guid].Stop();
						_timers.Remove(guid);
					}
				}
			}
		}
	}
}
