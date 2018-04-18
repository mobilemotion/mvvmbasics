using MvvmApp.Core.Viewmodels;
using MVVMbasics.Views;

namespace MvvmApp.UWP.Views
{
    public sealed partial class MainPage : BackButtonAwareBaseView, IBindableView<MainViewmodel>
	{
		public MainPage()
        {
            this.InitializeComponent();
        }

		public MainViewmodel Vm { get; set; }
    }
}
