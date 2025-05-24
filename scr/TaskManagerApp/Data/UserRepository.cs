using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TaskManager.Common;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Data
{
    /// <summary>
    /// Provides methods to perform CRUD operations on Users in the database.
    /// </summary>
    public class UserRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of <see cref="UserRepository"/> with the specified connection string.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves the list of users and admins separately from the database.
        /// </summary>
        /// <param name="users">Output list of non-admin user display names.</param>
        /// <param name="admins">Output list of admin user display names.</param>
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

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>List of <see cref="UserModel"/> objects.</returns>
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

        /// <summary>
        /// Inserts a new user record into the database with hashed password.
        /// </summary>
        /// <param name="user">The <see cref="UserModel"/> to insert.</param>
        /// <returns>True if insert succeeded; otherwise false.</returns>
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

        /// <summary>
        /// Deletes a user from the database by their ID.
        /// </summary>
        /// <param name="id">The user ID to delete.</param>
        /// <returns>True if deletion succeeded; otherwise false.</returns>
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

        /// <summary>
        /// Checks whether a given EmployeeCode already exists in the database.
        /// </summary>
        /// <param name="employeeCode">The employee code to check.</param>
        /// <returns>True if exists; otherwise false.</returns>
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

        /// <summary>
        /// Checks whether a given username already exists in the database.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>True if exists; otherwise false.</returns>
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

        /// <summary>
        /// Maps the current record of a <see cref="SqlDataReader"/> to a <see cref="UserModel"/> instance.
        /// </summary>
        /// <param name="reader">The SQL data reader positioned at a record.</param>
        /// <returns>A populated <see cref="UserModel"/>.</returns>
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
