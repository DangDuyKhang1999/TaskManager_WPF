using System;
using System.Data.SqlClient;
using System.Windows;

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
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT PasswordHash, IsAdmin FROM Users WHERE Username = @Username AND IsActive = 1";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader["PasswordHash"].ToString();
                            bool isAdmin = Convert.ToBoolean(reader["IsAdmin"]);

                            if (password == storedPasswordHash)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
