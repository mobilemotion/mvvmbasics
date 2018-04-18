using System.Reflection;
using MvvmApp.Core.Services;
using MvvmApp.XamarinForms.Services;
using MvvmApp.XamarinForms.Views;
using MVVMbasics;
using MVVMbasics.Services;
using Xamarin.Forms;

namespace MvvmApp.XamarinForms
{
	public partial class App : BaseApplication
	{
		public App ()
		{
			InitializeComponent();

			// MVVMbasics-specific initialization
			NavigatorService navigatorService = new NavigatorService();

			//TODO: Modify the following line to automatically or manually register your Views
			navigatorService.RegisterAll("MvvmApp.XamarinForms.Views.*", typeof(App).GetTypeInfo().Assembly);
			Services.Register(navigatorService);

			//TODO: Modify the following lines to automatically or manually register your Services
			Services.Register<MessageboxService>();
			Services.Register<SamplePlatformspecificService>();
			Services.Register<SamplePortableService>();

			SetStartupPage<MainPage>();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
