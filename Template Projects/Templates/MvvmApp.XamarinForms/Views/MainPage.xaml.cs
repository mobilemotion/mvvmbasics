using MvvmApp.Core.Viewmodels;
using MVVMbasics.Attributes;
using MVVMbasics.Views;
using Xamarin.Forms.Xaml;

namespace MvvmApp.XamarinForms.Views
{
	[MvvmNavigationTarget(typeof(MainViewmodel))]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : BaseView
	{
		public MainPage ()
		{
			InitializeComponent ();
		}
	}
}