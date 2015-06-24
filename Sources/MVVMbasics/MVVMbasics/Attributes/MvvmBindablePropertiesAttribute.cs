/*
 * (c) 2015 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Attributes
{
	/// <summary>
	/// Attribute to be applied to Model or Viewmodel classes which inherit from
	/// <see cref="MVVMbasics.Services.Parameter">BaseModel</see> or
	/// <see cref="MVVMbasics.Services.Parameter">BaseViewmodel</see>. All public properties within that class will
	/// raise the <c>PropertyChanged</c> event after being changed. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class MvvmBindablePropertiesAttribute : Attribute
	{
	}
}
