using Microsoft.Maui.Controls;
using ISUMPK2.Mobile.Components.Pages;
namespace ISUMPK2.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }
    }
}
