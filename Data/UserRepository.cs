using System.Data.SqlClient;
using TaskManager.Services;

namespace TaskManager.Data
{
    /// <summary>
    /// Repository class responsible for fetching user data from the database.
    /// </summary>
    public class UserRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of UserRepository with the specified connection string.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves all active usernames from the database, separated by user type (normal users and admins).
        /// </summary>
        /// <param name="users">Output list of usernames who are normal users (IsAdmin = 0).</param>
        /// <param name="admins">Output list of usernames who are admins (IsAdmin = 1).</param>
        public void GetUsersAndAdmins(out List<string> users, out List<string> admins)
        {
            users = new List<string>();
            admins = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // Query to select usernames and their admin status for all active users
                string query = "SELECT UserName, IsAdmin FROM Users WHERE IsActive = 1";

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                // Read each record and classify user as admin or normal user
                while (reader.Read())
                {
                    string username = reader["UserName"]?.ToString() ?? string.Empty;
                    bool isAdmin = reader["IsAdmin"] != DBNull.Value && (bool)reader["IsAdmin"];

                    if (isAdmin)
                        admins.Add(username);
                    else
                        users.Add(username);
                }
            }
            catch (SqlException ex)
            {
                // Log SQL-specific errors
                Logger.Instance.Error($"[SQL] {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other unexpected errors
                Logger.Instance.Error($" {ex.Message}");
            }
        }
    }
}
