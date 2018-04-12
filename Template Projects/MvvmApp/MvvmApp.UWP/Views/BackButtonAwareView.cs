using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using MVVMbasics.Views;
using MvvmApp.Core.Viewmodels;

namespace MvvmApp.UWP.Views
{
	public abstract class BackButtonAwareBaseView : BaseView
	{
		private NavigatorAwareBaseViewmodel _viewmodel;

		protected BackButtonAwareBaseView()
		{
			Loaded += BackButtonAwareBaseView_Loaded;

			// Register back-button handler
			if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
				SystemNavigationManager.GetForCurrentView().BackRequested += BackButtonAwareBaseView_BackRequested;
		}

		/// <summary>
		/// Sets the back-button's visibility depending on whether backward navigation is possible from this page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackButtonAwareBaseView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (_viewmodel != null)
			{
				_viewmodel.UpdateCanGoBack();
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
					_viewmodel.CanGoBack ?
					AppViewBackButtonVisibility.Visible :
					AppViewBackButtonVisibility.Collapsed;
			}
		}

		/// <summary>
		/// Navigates back to the previous page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackButtonAwareBaseView_BackRequested(object sender, BackRequestedEventArgs e)
		{
			if (_viewmodel != null && _viewmodel.CanGoBack)
			{
				e.Handled = true;
				_viewmodel.GoBackCommand.Execute();
			}
		}

		/// <summary>
		/// Stores a reference to this page's Viewmodel
		/// </summary>
		/// <param name="e"></param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			_viewmodel = DataContext as NavigatorAwareBaseViewmodel;
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			SystemNavigationManager.GetForCurrentView().BackRequested -= BackButtonAwareBaseView_BackRequested;
		}
	}
}
