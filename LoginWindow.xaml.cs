using System;
using System.Data.SqlClient;
using System.Windows;

namespace TaskManager
{
    public partial class LoginWindow : Window
    {
        // Chuỗi kết nối cơ sở dữ liệu
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

            // Kiểm tra nếu username và password không rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Username or password cannot be empty!";
                return;
            }

            // Kiểm tra thông tin đăng nhập
            if (AuthenticateUser(username, password))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // Nếu thông tin không đúng, hiển thị thông báo lỗi
                ErrorMessage.Text = "Invalid username or password.";
            }
        }

        // Hàm xác thực người dùng
        private bool AuthenticateUser(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Lấy thông tin người dùng từ cơ sở dữ liệu
                string query = "SELECT PasswordHash FROM Users WHERE Email = @Username AND IsActive = 1";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader["PasswordHash"].ToString();

                            // So sánh trực tiếp mật khẩu nhập vào và mật khẩu lưu trong cơ sở dữ liệu
                            return password == storedPasswordHash; // So sánh mật khẩu nhập vào và mật khẩu lưu
                        }
                    }
                }
            }
            return false; // Trả về false nếu không tìm thấy người dùng hoặc mật khẩu sai
        }
    }
}
