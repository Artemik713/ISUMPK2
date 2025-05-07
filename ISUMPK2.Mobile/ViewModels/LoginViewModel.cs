using ISUMPK2.Mobile.Services;
using System.Windows.Input;

namespace ISUMPK2.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        private string _username;
        private string _password;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            Title = "Вход в систему";
            LoginCommand = CreateCommand(LoginAsync);
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите имя пользователя и пароль";
                return;
            }

            var result = await _authService.LoginAsync(Username, Password);

            if (result)
            {
                await _navigationService.NavigateToAsync("///main");
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
            }
        }
    }
}
