using Microsoft.Maui; // Add this using directive for MauiWinUIApplication
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using ISUMPK2.Mobile;

namespace ISUMPK2.Mobile.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication // Correct base class for a Windows Maui app
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(); // Reference MauiProgram directly
    }
}
