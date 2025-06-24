using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISUMPK2.Mobile.ViewModels;
using ISUMPK2.Mobile.Services;

namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
