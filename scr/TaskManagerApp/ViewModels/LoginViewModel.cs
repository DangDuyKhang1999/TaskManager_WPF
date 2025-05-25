using System;
using System.Windows.Input;
using TaskManagerApp.Common;
using TaskManagerApp.Services;
using TaskManagerApp.Contexts;
using Microsoft.Data.SqlClient;

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
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
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
            // Validate input fields before attempting authentication
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = AppConstants.AppText.Message_LoginEmptyFields;
                Logger.Instance.Warning(ErrorMessage);
                return;
            }

            // Attempt to authenticate; log success or set error message on failure
            if (AuthenticateUser(Username, Password))
            {
                Logger.Instance.Information($"User '{Username}' authenticated successfully.");
                LoginSucceeded?.Invoke(); // Notify View to close
            }
            else
            {
                // If no specific error message set by AuthenticateUser, use generic
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage = AppConstants.AppText.Message_LoginInvalidCredentials;
                    Logger.Instance.Warning(ErrorMessage);
                }
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
                using var connection = new SqlConnection(AppConstants.Database.ConnectionString);
                connection.Open();

                const string query = @"
                    SELECT PasswordHash, IsAdmin, EmployeeCode 
                    FROM Users 
                    WHERE Username = @Username COLLATE Latin1_General_BIN AND IsActive = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    // No user found
                    ErrorMessage = AppConstants.AppText.Message_LoginInvalidCredentials;
                    Logger.Instance.Warning($"Authentication failed: user '{username}' not found or inactive.");
                    return false;
                }

                // Safely retrieve hashed password
                var storedHash = reader["PasswordHash"]?.ToString();
                if (string.IsNullOrEmpty(storedHash))
                {
                    ErrorMessage = AppConstants.AppText.Message_LoginInvalidCredentials;
                    Logger.Instance.Warning($"Authentication failed: no password hash for user '{username}'.");
                    return false;
                }

                // Verify password
                bool validPassword = false;
                try
                {
                    validPassword = BCrypt.Net.BCrypt.Verify(password, storedHash);
                }
                catch (Exception ex)
                {
                    // Hash verification error
                    Logger.Instance.Error(ex, callerMemberName: nameof(AuthenticateUser));
                    ErrorMessage = "Error validating credentials.";
                    return false;
                }

                if (!validPassword)
                {
                    ErrorMessage = AppConstants.AppText.Message_LoginInvalidCredentials;
                    Logger.Instance.Warning($"Authentication failed: invalid password for user '{username}'.");
                    return false;
                }

                // Retrieve other fields
                bool isAdmin = reader["IsAdmin"] is bool b && b;
                string? employeeCode = reader["EmployeeCode"]?.ToString();

                // Initialize session
                UserSession.Instance.Initialize(username, employeeCode ?? string.Empty, isAdmin);
                return true;
            }
            catch (SqlException sqlEx)
            {
                // Log SQL errors
                Logger.Instance.Error(sqlEx, callerMemberName: nameof(AuthenticateUser));
                ErrorMessage = "Database error during authentication.";
                return false;
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                Logger.Instance.Error(ex, callerMemberName: nameof(AuthenticateUser));
                ErrorMessage = "Unexpected error during authentication.";
                return false;
            }
        }
    }
}
