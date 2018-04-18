using MvvmApp.Core.Services;
using MVVMbasics.Attributes;
using MVVMbasics.Commands;
using MVVMbasics.Services;

namespace MvvmApp.Core.Viewmodels
{
	public sealed class MainViewmodel : NavigatorAwareBaseViewmodel
	{
		private readonly ISamplePlatformspecificService _platformspecificService;
		private readonly ISamplePortableService _portableService;
		private readonly INavigatorService _navigatorService;

		#region Bindable properties

		//TODO: Add properties and, if necessary, declare them as bindable, as shown in the following example:
		[MvvmBindable]
		public bool SampleProperty { get; set; }

		#endregion

		#region Bindable commands

		//TODO: Add commands, as shown in the following example
		public BaseCommand SampleCommand { get; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public MainViewmodel(ISamplePlatformspecificService platformspecificService, ISamplePortableService portableService, INavigatorService navigatorService) : base(navigatorService)
		{
			_platformspecificService = platformspecificService;
			_portableService = portableService;
			_navigatorService = navigatorService;

			#region Bindable properties

			//TODO: Assign default values to the bindable properties defined above
			SampleProperty = true;

			#endregion

			#region Bindable commands

			//TODO: Assign local methods and, if desired, conditions to the bindable commands defined above
			SampleCommand = CreateCommand(SampleMethod, () => SampleProperty);

			#endregion
		}

		private void SampleMethod()
		{
			//TODO: Create methods like this, one for each bindable command as defined above
		}

		/// <summary>
		/// Gets called when navigating to a page.
		/// </summary>
		/// <param name="uriParameters">List of parameters that were parsed from the URI.</param>
		/// <param name="parameters">List of parameters that were passed from the calling page.</param>
		/// <param name="viewState">Indicates the lifecycle state the View is about to reach.</param>
		public override void OnNavigatedTo(MVVMbasics.Services.ParameterList uriParameters,
										   MVVMbasics.Services.ParameterList parameters,
										   MVVMbasics.Services.ViewState viewState)
		{
			//TODO: Add code, or remove method (if you use this method, do not remove the following line!)
			base.OnNavigatedTo(uriParameters, parameters, viewState);
		}

		/// <summary>
		/// Gets called when the page is about to be closed, in order to return back to the previously shown page.
		/// </summary>
		/// <param name="viewState">Indicates the lifecycle state the View is about to reach.</param>
		/// <returns></returns>
		public override bool CancelNavigatingFrom(MVVMbasics.Services.ViewState viewState)
		{
			//TODO: Add code and return TRUE to cancel the closing process, or remove method (if you use this method, do not remove the following line!)
			return base.CancelNavigatingFrom(viewState);
		}

		/// <summary>
		/// Gets called when a page is closed.
		/// </summary>
		/// <param name="viewState">Indicates the lifecycle state the View is about to reach.</param>
		public override void OnNavigatedFrom(MVVMbasics.Services.ViewState viewState)
		{
			//TODO: Add code, or remove method (if you use this method, do not remove the following line!)
			base.OnNavigatedFrom(viewState);
		}
	}
}
