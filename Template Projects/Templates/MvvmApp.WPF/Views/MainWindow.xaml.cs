using MvvmApp.Core.Viewmodels;
using MVVMbasics.Attributes;
using MVVMbasics.Views;

namespace MvvmApp.WPF.Views
{
	[MvvmNavigationTarget(typeof(MainViewmodel))]
	public partial class MainWindow : BaseView
	{
		public MainWindow()
		{
			InitializeComponent();
		}
	}
}
