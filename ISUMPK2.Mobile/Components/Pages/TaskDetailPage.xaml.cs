using ISUMPK2.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly TaskDetailViewModel _viewModel;

        public TaskDetailPage(TaskDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}