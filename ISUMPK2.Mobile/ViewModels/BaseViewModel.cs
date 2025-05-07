using Microsoft.CSharp.RuntimeBinder;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ISUMPK2.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private string _title;
        private string _errorMessage;
        private bool _hasError;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ExecuteAsync(Func<Task> action, string errorMessage = "Произошла ошибка")
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                await action();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{errorMessage}: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand CreateCommand(Func<Task> execute)
        {
            return new Command(async () => await ExecuteAsync(execute));
        }

        public ICommand CreateCommand(Action execute)
        {
            return new Command(execute);
        }

        public ICommand CreateCommand<T>(Func<T, Task> execute)
        {
            return new Command<T>(async (param) => await ExecuteAsync(() => execute(param)));
        }

        public ICommand CreateCommand<T>(Action<T> execute)
        {
            return new Command<T>(execute);
        }
        public virtual Task OnAppearingAsync()
        {
            return Task.CompletedTask;
        }

    }
}
