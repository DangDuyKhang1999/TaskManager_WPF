using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TaskManager.Common;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void GetUsersAndAdmins(out List<string> users, out List<string> admins)
        {
            users = new List<string>();
            admins = new List<string>();

            try
            {
                Logger.Instance.Information(AppConstants.Logging.Information + " Opening database connection to load user list and admin list.");
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_GetUsersAndAdmins, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string displayName = reader["DisplayName"]?.ToString() ?? string.Empty;
                    bool isAdmin = reader["IsAdmin"] != DBNull.Value && (bool)reader["IsAdmin"];

                    if (isAdmin)
                        admins.Add(displayName);
                    else
                        users.Add(displayName);
                }
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" Unexpected error: {ex.Message}");
            }
        }

        public List<UserModel> GetAllUsers()
        {
            var users = new List<UserModel>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_GetAllUsers, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(MapReaderToUser(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" Failed to load users: {ex.Message}");
            }

            return users;
        }

        public bool InsertUser(UserModel user)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                const string query = @"
INSERT INTO Users (EmployeeCode, Username, PasswordHash, DisplayName, Email, IsAdmin, IsActive, CreatedAt)
VALUES (@EmployeeCode, @Username, @PasswordHash, @DisplayName, @Email, @IsAdmin, @IsActive, GETDATE())";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeCode", user.EmployeeCode);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                command.Parameters.AddWithValue("@DisplayName", user.DisplayName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);

                command.ExecuteNonQuery();
                Logger.Instance.Information(AppConstants.Logging.Success + $" Inserted user successfully. Username: {user.Username}");
                return true;
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" SQL Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" Error inserting user: {ex.Message}");
                return false;
            }
        }

        public bool DeleteUserById(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_DeleteUserById, connection);
                command.Parameters.AddWithValue("@Id", id);

                int affected = command.ExecuteNonQuery();
                return affected > 0;
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" SQL Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" Error deleting user by ID: {ex.Message}");
                return false;
            }
        }

        public bool DoesEmployeeCodeExist(string employeeCode)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_CheckEmployeeCode, connection);
                command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" SQL Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" Error checking EmployeeCode duplication: {ex.Message}");
                return false;
            }
        }

        public bool DoesUsernameExist(string username)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_CheckUsername, connection);
                command.Parameters.AddWithValue("@Username", username);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" SQL Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(AppConstants.Logging.Error + $" Error checking Username duplication: {ex.Message}");
                return false;
            }
        }

        private UserModel MapReaderToUser(SqlDataReader reader)
        {
            return new UserModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                EmployeeCode = reader["EmployeeCode"]?.ToString() ?? string.Empty,
                Username = reader["Username"]?.ToString() ?? string.Empty,
                PasswordHash = reader["PasswordHash"]?.ToString() ?? string.Empty,
                DisplayName = reader["DisplayName"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
