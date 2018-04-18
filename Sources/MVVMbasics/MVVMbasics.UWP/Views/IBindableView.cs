/*
 * (c) 2015-2018 Andreas Kuntner
 */

 using MVVMbasics.Viewmodels;

namespace MVVMbasics.Views
{
	/// <summary>
	/// Contract that forces a View to include a public <code>Vm</code> property representing the Viewmodel, and
	/// that is used by <see cref="MVVMbasics.Services.INavigatorService">NavigatorService</see>see> to identify
	/// matching pairs of Views and Viewmodels.
	/// </summary>
	/// <typeparam name="T">Type of Viewmodel that shall be used as this View's <code>DataContext</code>.</typeparam>
	public interface IBindableView<T> where T : BaseViewmodel
	{
		/// <summary>
		/// Represents this View's Viewmodel (will automatically be synchronised to always point to the current
		/// <code>DataContext</code> instance), to be used by x:Bind markup extension.
		/// </summary>
		T Vm { get; set; }
	}
}
