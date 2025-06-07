using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigationService;

        private bool _isLoading;
        private UserModel _user;
        private bool _isDarkMode;

        public string Title => "Мой профиль";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserModel User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    _settingsService.SetSetting("DarkMode", value);
                    ApplyTheme();
                }
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand RefreshCommand { get; }

        public ProfileViewModel(IAuthService authService, ISettingsService settingsService, INavigationService navigationService)
        {
            _authService = authService;
            _settingsService = settingsService;
            _navigationService = navigationService;

            LogoutCommand = new Command(async () => await LogoutAsync());
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            RefreshCommand = new Command(async () => await LoadUserDataAsync());

            // Загрузка настроек
            _isDarkMode = _settingsService.GetSetting<bool>("DarkMode", false);
        }

        public async Task LoadUserDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                User = await _authService.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить данные профиля: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LogoutAsync()
        {
            var confirm = await Shell.Current.DisplayAlert("Выход", "Вы действительно хотите выйти из аккаунта?", "Да", "Нет");

            if (confirm)
            {
                await _authService.LogoutAsync();
                _navigationService.NavigateToAsync("///login", true);
            }
        }

        private async Task ChangePasswordAsync()
        {
            _navigationService.NavigateToAsync("change-password");
        }

        private void ApplyTheme()
        {
            // Реализация изменения темы
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}