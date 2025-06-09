using ISUMPK2.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class MaterialListPage : ContentPage
    {
        private readonly MaterialListViewModel _viewModel;

        public MaterialListPage(MaterialListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadMaterialsAsync();
        }
    }
}