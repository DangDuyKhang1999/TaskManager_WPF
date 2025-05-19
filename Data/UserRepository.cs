using System.Data.SqlClient;
using TaskManager.Common;
using TaskManager.Services;
using static TaskManager.Common.AppConstants;

namespace TaskManager.Data
{
    /// <summary>
    /// Repository class responsible for fetching user data from the database.
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
            Logger.Instance.Info("UserRepository initialized with connection string.");
        }

        /// <summary>
        /// Retrieves all active users from the database, separated by user type (normal users and admins).
        /// </summary>
        /// <param name="users">Output list of usernames who are normal users (IsAdmin = 0).</param>
        /// <param name="admins">Output list of usernames who are admins (IsAdmin = 1).</param>
        public void GetUsersAndAdmins(out List<string> users, out List<string> admins)
        {
            users = new List<string>();
            admins = new List<string>();

            try
            {
                Logger.Instance.Info("Opening database connection to load user list and admin list.");
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                Logger.Instance.Info("Executing query to retrieve active users and admins.");
                const string query = "SELECT DisplayName, IsAdmin FROM Users WHERE IsActive = 1";
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                // Read each record and classify user as admin or normal user
                while (reader.Read())
                {
                    string displayName = reader["DisplayName"]?.ToString() ?? string.Empty;
                    bool isAdmin = reader["IsAdmin"] != DBNull.Value && (bool)reader["IsAdmin"];

                    if (isAdmin)
                    {
                        admins.Add(displayName);
                    }
                    else
                    {
                        users.Add(displayName);
                    }
                }
                Logger.Instance.Info("Successfully retrieved list of users and admins.");
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error($"[SQL] {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{AppConstants.Logging.Message_UnexpectedError}: {ex.Message}");
            }
        }
    }
}
