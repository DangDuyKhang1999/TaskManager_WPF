using System;
using System.Data.SqlClient;
using System.Windows;

namespace TaskManager
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
                SaveLoginHistory(username);
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

                string query = "SELECT PasswordHash, IsAdmin FROM Users WHERE Email = @Username AND IsActive = 1";
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

        private void SaveLoginHistory(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Xóa toàn bộ bản ghi trong bảng UserLoginHistory
                    string deleteQuery = "DELETE FROM UserLoginHistory";
                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Thêm bản ghi mới
                    string insertQuery = @"
                INSERT INTO UserLoginHistory (Id, Email, IsAdmin)
                SELECT 1, @Email, IsAdmin FROM Users WHERE Email = @Email
            ";
                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Email", email);
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu UserLoginHistory: {ex.Message}");
                }
            }
        }
    }
}
