using ISUMPK2.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class NotificationListPage : ContentPage
    {
        private readonly NotificationListViewModel _viewModel;

        public NotificationListPage(NotificationListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadNotificationsAsync();
        }
    }
}