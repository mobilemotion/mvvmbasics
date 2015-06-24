/*
 * (c) 2014-2015 Andreas Kuntner
 */

using System;
using System.Collections.Generic;
using MVVMbasics.Attributes;
using System.Threading.Tasks;
using System.Threading;

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
		private readonly Dictionary<Guid, CancellationTokenSource> _timers = new Dictionary<Guid, CancellationTokenSource>();

		/// <summary>
		/// Creates a new timer that will run once, and starts it.
		/// <param name="interval">Timer interval</param>
		/// <param name="callback">Method to be executed on the timer tick event.</param>
		/// </summary>
		public Guid StartOnce(TimeSpan interval, Action callback)
		{
			Guid id = Guid.NewGuid();
			CancellationTokenSource tokenSource = new CancellationTokenSource(); 
#pragma warning disable 4014
			PutTaskDelay ((int)interval.TotalMilliseconds, callback, tokenSource.Token);
#pragma warning restore 4014
			_timers.Add(id, tokenSource);
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
			CancellationTokenSource tokenSource = new CancellationTokenSource();
#pragma warning disable 4014
			PutTaskDelay ((int)interval.TotalMilliseconds, callback, tokenSource.Token, true);
#pragma warning restore 4014
			_timers.Add(id, tokenSource);
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
					timer.Value.Cancel ();
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
						_timers [guid].Cancel ();
						_timers.Remove(guid);
					}
				}
			}
		}

		/// <summary>
		/// Actually calls the provided action after a delay.
		/// </summary>
		/// <param name="milliseconds">Interval in milliseconds.</param>
		/// <param name="action">Method to be executed after the delay.</param>
		/// <param name="token">Cancellation token that allows to cancel the timer.</param>
		/// <param name="loop">Determins whether the timer shall be restarted after the given action has been executed.</param>
		private async Task PutTaskDelay(int milliseconds, Action action, CancellationToken token, bool loop = false)
		{ 
			try
			{
				do
				{
					await Task.Delay(milliseconds, token);
					action.Invoke();
				} while (loop);
			}
			catch (TaskCanceledException) { }
		}

	}
}
