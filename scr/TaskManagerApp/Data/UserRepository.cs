using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TaskManagerApp.Common;
using TaskManagerApp.Models;
using TaskManagerApp.Services;

namespace TaskManagerApp.Data
{
    /// <summary>
    /// Handles CRUD operations for User entities in the database.
    /// </summary>
    public class UserRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves non-admin users and admin users separately.
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

                // Establish connection and execute query to fetch users
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_GetUsersAndAdmins, connection);
                using var reader = command.ExecuteReader();

                // Process each row, separating admins and users based on IsAdmin flag
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
        /// <returns>A list of all users as <see cref="UserModel"/> objects.</returns>
        public List<UserModel> GetAllUsers()
        {
            var users = new List<UserModel>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_GetAllUsers, connection);
                using var reader = command.ExecuteReader();

                // Map each database record to a UserModel instance
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
        /// Inserts a new user record with a hashed password.
        /// </summary>
        /// <param name="user">The user data to insert.</param>
        /// <returns>True if insertion is successful; otherwise, false.</returns>
        public bool InsertUser(UserModel user)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // Hash the plaintext password before storing
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                const string query = @"
INSERT INTO Users (EmployeeCode, Username, PasswordHash, DisplayName, Email, IsAdmin, IsActive, CreatedAt)
VALUES (@EmployeeCode, @Username, @PasswordHash, @DisplayName, @Email, @IsAdmin, @IsActive, GETDATE())";

                using var command = new SqlCommand(query, connection);
                // Add parameters with null checks for optional fields
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
        /// Deletes a user by their unique identifier.
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

                // Execute deletion and check if any rows were affected
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
        /// Determines if an EmployeeCode already exists in the database.
        /// </summary>
        /// <param name="employeeCode">Employee code to check.</param>
        /// <returns>True if the code exists; otherwise false.</returns>
        public bool DoesEmployeeCodeExist(string employeeCode)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(AppConstants.Database.Query_CheckEmployeeCode, connection);
                command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

                // Execute scalar query returning count of matching records
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
        /// Determines if a username already exists in the database.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if the username exists; otherwise false.</returns>
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
        /// Converts a data record from the database into a <see cref="UserModel"/> object.
        /// </summary>
        /// <param name="reader">The SQL data reader positioned at a valid record.</param>
        /// <returns>A user model populated with the record's data.</returns>
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
