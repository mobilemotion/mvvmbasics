using MVVMbasics.Commands;
using MVVMbasics.Services;
using MVVMbasics.Viewmodels;

namespace MvvmApp.Core.Viewmodels
{
	/// <summary>
	/// Wrapper class for MVVMbasic's BaseViewmodel that includes members for backwards navigation to be used in Windows
	/// Store Apps. If you don't need to include back buttons in your App (e.g., if you only target WPF platform),
	/// delete this class and let all Viewmodels inherit directly from BaseViewmodel.
	/// </summary>
	public class NavigatorAwareBaseViewmodel : BaseViewmodel
	{
		private readonly INavigatorService _navigatorService;

		/// <summary>
		/// Flag indicating whether the current View might navigate backwards. Useful for binding a back button's visibility.
		/// </summary>
		private bool _canGoBack = false;
		public bool CanGoBack
		{
			get => _canGoBack;
			set => Set(ref _canGoBack, value);
		}

		/// <summary>
		/// Command that navigated back, if possible.
		/// </summary>
		public BaseCommand GoBackCommand { get; }

		public NavigatorAwareBaseViewmodel(INavigatorService navigatorService)
		{
			_navigatorService = navigatorService;
			GoBackCommand = CreateCommand(GoBack, () => CanGoBack);
		}

		public override void OnNavigatedTo(ParameterList uriParameters, ParameterList parameters, ViewState viewState)
		{
			base.OnNavigatedTo(uriParameters, parameters, viewState);
			UpdateCanGoBack();
		}

		/// <summary>
		/// Method that is called from GoBackCommand.
		/// </summary>
		private void GoBack()
		{
			_navigatorService.NavigateBack();
		}

		/// <summary>
		/// Method that should be called manually after manipulating the current Viewmodel's back stack.
		/// </summary>
		public void UpdateCanGoBack()
		{
			CanGoBack = _navigatorService.CanGoBack();
		}
	}
}
