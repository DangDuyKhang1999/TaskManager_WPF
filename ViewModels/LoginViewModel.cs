using System;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Services;
using TaskManager.Contexts;

namespace TaskManager.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;

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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public event Action LoginSucceeded;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = AppConstants.AppText.Message_LoginEmptyFields;
                Logger.Instance.Warning(ErrorMessage);
                return;
            }

            if (AuthenticateUser(Username, Password))
            {
                LoginSucceeded?.Invoke(); // Notify View to close
            }
            else
            {
                ErrorMessage = AppConstants.AppText.Message_LoginInvalidCredentials;
                Logger.Instance.Warning(ErrorMessage);
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using var connection = new System.Data.SqlClient.SqlConnection(AppConstants.Database.ConnectionString);
                connection.Open();

                string query = @"
                    SELECT PasswordHash, IsAdmin, EmployeeCode 
                    FROM Users 
                    WHERE Username = @Username COLLATE Latin1_General_BIN AND IsActive = 1";

                using var command = new System.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var passwordHash = reader["PasswordHash"]?.ToString();
                    bool isAdmin = reader["IsAdmin"] is bool b && b;
                    string employeeCode = reader["EmployeeCode"]?.ToString();

                    // So sánh mật khẩu bằng BCrypt
                    if (!string.IsNullOrEmpty(passwordHash) && BCrypt.Net.BCrypt.Verify(password, passwordHash))
                    {
                        UserSession.Instance.Initialize(username, employeeCode, isAdmin);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{AppConstants.Logging.LoginAuthFailed} {ex.Message}");
                ErrorMessage = AppConstants.Logging.LoginFailed;
            }

            return false;
        }
    }
}
