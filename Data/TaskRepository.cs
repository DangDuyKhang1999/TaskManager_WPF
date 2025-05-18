using System.Data.SqlClient;
using TaskManager.Contexts;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Data
{
    /// <summary>
    /// Repository class responsible for accessing Task data from the SQL Server database.
    /// </summary>
    public class TaskRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of TaskRepository with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves all tasks from the database along with their assignees.
        /// If the current user is not an admin, only tasks assigned to that user are returned.
        /// </summary>
        /// <returns>A list of TaskModel representing all relevant tasks.</returns>
        public List<TaskModel> GetAllTasks()
        {
            var tasks = new List<TaskModel>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // SQL query to select tasks with their assignees' display names.
                string query = @"
                    SELECT T.Code, T.Title, T.Description, 
                           ISNULL(U.DisplayName, '(Unassigned)') AS Assignee, 
                           T.Status, T.DueDate, T.Priority, T.CreatedAt, T.UpdatedAt
                    FROM Tasks T
                    LEFT JOIN Users U ON T.AssigneeId = U.Id";

                // If the user is not admin, restrict tasks to only those assigned to this user.
                if (!UserSession.Instance.IsAdmin)
                {
                    query += " WHERE U.UserName = @UserName";
                }

                using var command = new SqlCommand(query, connection);

                if (!UserSession.Instance.IsAdmin)
                {
                    command.Parameters.AddWithValue("@UserName", UserSession.Instance.UserName);
                }

                using var reader = command.ExecuteReader();

                // Read data and map to TaskModel objects.
                while (reader.Read())
                {
                    tasks.Add(new TaskModel
                    {
                        Id = reader["Code"]?.ToString() ?? string.Empty,
                        Title = reader["Title"]?.ToString() ?? string.Empty,
                        Description = reader["Description"]?.ToString() ?? string.Empty,
                        Assignee = reader["Assignee"]?.ToString() ?? "(Unassigned)",
                        Status = GetStatusString(Convert.ToInt32(reader["Status"])),
                        DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue,
                        Priority = Convert.ToInt32(reader["Priority"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                    });
                }
            }
            catch (SqlException ex)
            {
                // Log SQL-related errors.
                Logger.Instance.Error($" [SQL] {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log other unexpected errors.
                Logger.Instance.Error($" {ex.Message}");
            }

            return tasks;
        }

        /// <summary>
        /// Converts an integer status code to its descriptive string representation.
        /// </summary>
        /// <param name="status">The numeric status code.</param>
        /// <returns>A string describing the status.</returns>
        private string GetStatusString(int status)
        {
            return status switch
            {
                0 => "Not Started",
                1 => "In Progress",
                2 => "Completed",
                _ => "Unknown"
            };
        }
    }
}
