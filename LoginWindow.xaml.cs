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
        private void SaveLoginHistory(string username)
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

                    // Lấy IsAdmin từ bảng Users
                    string isAdminQuery = "SELECT IsAdmin FROM Users WHERE Username = @Username";
                    bool isAdmin = false; // Default to false if no record is found
                    using (var isAdminCommand = new SqlCommand(isAdminQuery, connection))
                    {
                        isAdminCommand.Parameters.AddWithValue("@Username", username);

                        using (var reader = isAdminCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                            }
                        }
                    }

                    // Thêm bản ghi mới vào bảng UserLoginHistory
                    string insertQuery = @"
            INSERT INTO UserLoginHistory (Id, Username, IsAdmin)
            VALUES (1, @Username, @IsAdmin)"; // Thêm IsAdmin vào câu lệnh SQL
                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Username", username);
                        insertCommand.Parameters.AddWithValue("@IsAdmin", isAdmin); // Chuyển IsAdmin vào câu lệnh SQL
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
