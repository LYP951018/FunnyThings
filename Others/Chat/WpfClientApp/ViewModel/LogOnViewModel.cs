using Client;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;
using WpfClientApp.ViewModel;

namespace WpfClientApp
{
    class LogOnViewModel
    {
        private string _userIdInput;
        private DelegateCommand _logOnCommand = null;
        private bool _isInputValid = false;

        public string UserIdInput
        {
            get { return _userIdInput; }
            set
            {
                _userIdInput = value;
                IsInputValid = true;
            }
        }

        public ICommand LogOnCommand => _logOnCommand;
        public ICommand ValidateErrorCommand { get; set; }

        private bool IsInputValid
        {
            get { return _isInputValid; }
            set
            {
                _isInputValid = value;
                _logOnCommand.RaiseCanExecuteChanged();
            }
        }

        public LogOnViewModel()
        {
            ValidateErrorCommand = new DelegateCommand(() => IsInputValid = false);
            _logOnCommand = new DelegateCommand(() => OnLogOn(), () => IsInputValid);
        }

        private async void OnLogOn()
        {
            var client = new ChatClient();
            await client.StartAsync();
            try
            {
                var info = await client.LogOn(int.Parse(_userIdInput));
                var current = Application.Current.MainWindow;
                var mainVM = new MainViewModel(client, info);
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.DataContext = mainVM;
                current.Close();
                mainWindow.Show();
            }
            catch (DuplicateIdException)
            {
            }
        }
    }
}
