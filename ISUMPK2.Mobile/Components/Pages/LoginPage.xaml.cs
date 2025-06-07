using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISUMPK2.Mobile.ViewModels;


namespace ISUMPK2.Mobile.Components.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Назначаем контекст данных для страницы
            this.BindingContext = new LoginViewModel();
        }
    }
}