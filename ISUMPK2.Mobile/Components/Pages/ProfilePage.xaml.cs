using ISUMPK2.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadUserDataAsync();
        }
    }
}