/*
 * (c) 2013-2018 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Attributes
{
	/// <summary>
	/// Attribute to be applied to Viewmodel classes. Define whether CommandAutobinding (the automatic binding of 
	/// <see cref="MVVMbasics.Commands.BaseCommand">BaseCommands</see> to matching methods in the Viewmodel) is 
	/// activated or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, 
		AllowMultiple = false, 
		Inherited = false)]
	public class MvvmCommandAutobindingAttribute : Attribute
	{
		private readonly bool _autobinding = false;

		public MvvmCommandAutobindingAttribute()
		{
			_autobinding = true;
		}

		public MvvmCommandAutobindingAttribute(bool autobinding)
		{
			_autobinding = autobinding;
		}

		internal bool GetAutobinding()
		{
			return _autobinding;
		}
	}
}
