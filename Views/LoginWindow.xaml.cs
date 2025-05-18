using System;
using System.Data.SqlClient;
using System.Windows;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class LoginWindow : Window
    {
        private readonly string _connectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";

        public LoginWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

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
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErrorMessage.Text = "Invalid username or password.";
                Logger.Instance.Warning("Invalid username or password.");
            }
        }
        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                SELECT PasswordHash, IsAdmin 
                FROM Users 
                WHERE Username = @Username COLLATE Latin1_General_BIN
                AND IsActive = 1";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var passwordHashObj = reader["PasswordHash"];
                                var isAdminObj = reader["IsAdmin"];

                                if (passwordHashObj != DBNull.Value && isAdminObj != DBNull.Value)
                                {
                                    string storedPasswordHash = (string)passwordHashObj;
                                    bool isAdmin = (bool)isAdminObj;

                                    if (string.Equals(password, storedPasswordHash, StringComparison.Ordinal))
                                    {
                                        UserSession.Instance?.Initialize(username, isAdmin);
                                        return true;
                                    }
                                }
                            }
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
