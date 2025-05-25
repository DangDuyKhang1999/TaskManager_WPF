using System;
using Microsoft.Data.SqlClient;
using TaskManagerApp.Common;
using TaskManagerApp.Models;

namespace TaskManagerApp.Services
{
    /// <summary>
    /// Provides methods to interact with tasks in the database.
    /// </summary>
    public class TaskService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskService"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <exception cref="ArgumentNullException">Thrown if connectionString is null or empty.</exception>
        public TaskService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string must not be null or empty.");

            _connectionString = connectionString;
        }

        /// <summary>
        /// Adds a new task to the database.
        /// </summary>
        /// <param name="task">The task model to add.</param>
        /// <returns>True if the task was added successfully; otherwise, false.</returns>
        public bool AddTask(TaskModel task)
        {
            if (task == null)
            {
                Logger.Instance.Error("Cannot add null task.");
                return false;
            }

            const string query = @"
                INSERT INTO Tasks
                (Code, Title, Description, Status, DueDate, Priority, ReporterIdDisplayName, AssigneeDisplayName, CreatedAt, UpdatedAt)
                VALUES
                (@Code, @Title, @Description, @Status, @DueDate, @Priority, @ReporterIdDisplayName, @AssigneeDisplayName, @CreatedAt, @UpdatedAt);
            ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand(query, connection);

                // Add parameters with null checks, using DBNull for empty
                command.Parameters.AddWithValue("@Code", task.Code ?? string.Empty);
                command.Parameters.AddWithValue("@Title", task.Title ?? string.Empty);
                command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(task.Description) ? DBNull.Value : (object)task.Description);
                command.Parameters.AddWithValue("@Status", task.Status);
                command.Parameters.AddWithValue("@DueDate", task.DueDate == DateTime.MinValue ? DBNull.Value : (object)task.DueDate);
                command.Parameters.AddWithValue("@Priority", task.Priority);
                command.Parameters.AddWithValue("@ReporterIdDisplayName", task.ReporterDisplayName ?? string.Empty);
                command.Parameters.AddWithValue("@AssigneeDisplayName", task.AssigneeDisplayName ?? string.Empty);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                connection.Open();

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected <= 0)
                {
                    Logger.Instance.Warning($"AddTask executed but no rows were affected. Task Code: {task.Code}");
                    return false;
                }

                Logger.Instance.Information($"Task added successfully. Code: {task.Code}");
                return true;
            }
            catch (SqlException sqlEx)
            {
                // Log SQL-specific errors for troubleshooting
                Logger.Instance.Error(sqlEx, callerMemberName: nameof(AddTask));
                return false;
            }
            catch (Exception ex)
            {
                // Log any other errors
                Logger.Instance.Error(ex, callerMemberName: nameof(AddTask));
                return false;
            }
        }
    }
}
