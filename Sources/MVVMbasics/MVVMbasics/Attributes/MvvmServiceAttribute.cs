/*
 * (c) 2013-2018 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Attributes
{
	/// <summary>
	/// Attribute to be applied to service classes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, 
		AllowMultiple = false, 
		Inherited = false)]
	public class MvvmServiceAttribute : Attribute
	{
	}
}
