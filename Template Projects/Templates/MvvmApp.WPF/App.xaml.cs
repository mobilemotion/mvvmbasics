using MvvmApp.Core.Services;
using MvvmApp.WPF.Services;
using MVVMbasics;
using MVVMbasics.Services;

namespace MvvmApp.WPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : BaseApplication
	{
		public App()
		{
			// MVVMbasics-specific initialization
			NavigatorService navigatorService = new NavigatorService();
			//TODO: Modify the following line to automatically or manually register your Views
			navigatorService.RegisterAll("MvvmApp.WPF.Views.*");
			base.Services.Register(navigatorService);
			base.Services.Register<MessageboxService>();
			//TODO: Modify the following lines to automatically or manually register your Services
			base.Services.Register<SamplePlatformspecificService>();
			base.Services.Register<SamplePortableService>();
		}
	}
}
