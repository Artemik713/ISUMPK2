using ISUMPK2.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class TaskListPage : ContentPage
    {
        private readonly TaskListViewModel _viewModel;

        public TaskListPage(TaskListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadTasksAsync();
        }
    }
}