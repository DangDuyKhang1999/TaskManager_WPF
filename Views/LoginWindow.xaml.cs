using System;
using System.Data.SqlClient;
using System.Windows;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class LoginWindow : Window
    {
        // Connection string to the database
        private readonly string _connectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";

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
                ErrorMessage.Text = "Username or password cannot be empty!";
                Logger.Instance.Warning("Username or password cannot be empty!");
                return;
            }

            if (AuthenticateUser(username, password))
            {
                this.DialogResult = true; // Indicate successful login
                this.Close();
            }
            else
            {
                ErrorMessage.Text = "Invalid username or password.";
                Logger.Instance.Warning("Invalid username or password.");
            }
        }

        /// <summary>
        /// Authenticates user credentials against the database.
        /// </summary>
        /// <param name="username">Username input</param>
        /// <param name="password">Password input</param>
        /// <returns>True if authentication succeeds, otherwise false</returns>
        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = @"
                    SELECT PasswordHash, IsAdmin 
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

                    if (passwordHashObj != DBNull.Value && isAdminObj != DBNull.Value)
                    {
                        string storedPasswordHash = (string)passwordHashObj;
                        bool isAdmin = (bool)isAdminObj;

                        // Compare password directly - consider replacing with secure hash check
                        if (string.Equals(password, storedPasswordHash, StringComparison.Ordinal))
                        {
                            UserSession.Instance.Initialize(username, isAdmin);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Authentication failed: " + ex.Message);
                ErrorMessage.Text = "An error occurred during login. Please contact admin.";
            }

            return false;
        }
    }
}
