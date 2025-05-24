using System;
using System.Windows.Input;
using TaskManagerApp.Common;
using TaskManagerApp.Services;
using TaskManagerApp.Contexts;

namespace TaskManagerApp.ViewModels
{
    /// <summary>
    /// ViewModel for login screen handling user authentication.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Gets or sets the username input by the user.
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// Gets or sets the password input by the user.
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>
        /// Gets or sets the error message displayed to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Command executed when the user attempts to log in.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Event invoked when login is successful.
        /// </summary>
        public event Action? LoginSucceeded;

        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        /// <summary>
        /// Executes the login process.
        /// Validates input, attempts authentication, and handles success or failure.
        /// </summary>
        /// <param name="parameter">Command parameter (unused).</param>
        private void ExecuteLogin(object? parameter)
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

        /// <summary>
        /// Authenticates the user by verifying credentials against the database.
        /// </summary>
        /// <param name="username">Username input.</param>
        /// <param name="password">Password input.</param>
        /// <returns>True if authentication succeeds; otherwise, false.</returns>
        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(AppConstants.Database.ConnectionString);
                connection.Open();

                string query = @"
                    SELECT PasswordHash, IsAdmin, EmployeeCode 
                    FROM Users 
                    WHERE Username = @Username COLLATE Latin1_General_BIN AND IsActive = 1";

                using var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var passwordHash = reader["PasswordHash"]?.ToString();
                    bool isAdmin = reader["IsAdmin"] is bool b && b;
                    string? employeeCode = reader["EmployeeCode"]?.ToString();

                    if (!string.IsNullOrEmpty(passwordHash) && BCrypt.Net.BCrypt.Verify(password, passwordHash))
                    {
                        UserSession.Instance.Initialize(username, employeeCode ?? string.Empty, isAdmin);
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
