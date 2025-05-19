using System;
using System.Data.SqlClient;
using System.Windows;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class LoginWindow : Window
    {
        // Connection string to the database

        public LoginWindow()
        {
            InitializeComponent();
            // Center the login window on screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        /// <summary>
        /// Handles the click event of the login button.
        /// Validates input and attempts user authentication.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = AppConstants.AppText.Message_LoginEmptyFields;
                Logger.Instance.Warning(AppConstants.AppText.Message_LoginEmptyFields);
                return;
            }

            if (AuthenticateUser(username, password))
            {
                this.DialogResult = true; // Indicate successful login
                this.Close();
            }
            else
            {
                ErrorMessage.Text = AppConstants.AppText.Message_LoginInvalidCredentials;
                Logger.Instance.Warning(AppConstants.AppText.Message_LoginInvalidCredentials);
            }
        }

        /// <summary>
        /// Authenticates the user by verifying the provided username and password
        /// against the stored credentials in the database.
        /// </summary>
        /// <param name="username">The username entered by the user.</param>
        /// <param name="password">The password entered by the user (in plain text).</param>
        /// <returns>
        /// True if the username exists, the password matches, and the user is active; 
        /// otherwise, false.
        /// </returns>
        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using var connection = new SqlConnection(AppConstants.Database.ConnectionString);
                connection.Open();

                string query = @"
            SELECT PasswordHash, IsAdmin, EmployeeCode 
            FROM Users 
            WHERE Username = @Username COLLATE Latin1_General_BIN
            AND IsActive = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var passwordHashObj = reader["PasswordHash"];
                    var isAdminObj = reader["IsAdmin"];
                    var employeeCodeObj = reader["EmployeeCode"];

                    if (passwordHashObj != DBNull.Value && isAdminObj != DBNull.Value && employeeCodeObj != DBNull.Value)
                    {
                        string storedPasswordHash = (string)passwordHashObj;
                        bool isAdmin = (bool)isAdminObj;
                        string employeeCode = (string)employeeCodeObj;

                        // Compare password directly - consider replacing with secure hash check
                        if (string.Equals(password, storedPasswordHash, StringComparison.Ordinal))
                        {
                            UserSession.Instance.Initialize(username, employeeCode, isAdmin);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{AppConstants.Logging.Message_LoginAuthFailed} {ex.Message}");
                ErrorMessage.Text = AppConstants.Logging.Message_LoginFailed;
            }

            return false;
        }
    }
}
