/*
 * (c) 2015 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Attributes
{
	/// <summary>
	/// Attribute to be applied to Model or Viewmodel properties. All public properties within classes that are
	/// marked with the <see cref="MVVMbasics.Services.Parameter">BaseModel</see> or
	/// <see cref="MVVMbasics.Services.Parameter">BaseViewmodel</see> will not raise the <c>PropertyChanged</c>
	/// event after being changed. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MvvmBindableIgnoreAttribute : Attribute
	{
	}
}
