/*
 * (c) 2014-2018 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Helpers
{
	public interface IDispatcherHelper
	{
		void RunOnMainThread(Action action);
		bool IsRunningOnMainThread();
	}
}
