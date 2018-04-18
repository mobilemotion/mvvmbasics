/*
 * (c) 2015-2018 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Attributes
{
	/// <summary>
	/// Attribute to be applied to Model or Viewmodel properties. All public properties within classes that inherit
	/// from <see cref="MVVMbasics.Services.Parameter">BaseModel</see> or
	/// <see cref="MVVMbasics.Services.Parameter">BaseViewmodel</see> which are marked with this attribute will raise
	/// the <c>PropertyChanged</c> event after being changed. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MvvmBindableAttribute : Attribute
	{
	}
}
