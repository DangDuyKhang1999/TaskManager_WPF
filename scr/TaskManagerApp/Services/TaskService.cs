using System;
using Microsoft.Data.SqlClient;
using TaskManager.Models;

namespace TaskManager.Services
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
        public TaskService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Adds a new task to the database.
        /// </summary>
        /// <param name="task">The task model to add.</param>
        /// <returns>True if the task was added successfully; otherwise, false.</returns>
        public bool AddTask(TaskModel task)
        {
            var query = @"
                INSERT INTO Tasks
                (Code, Title, Description, Status, DueDate, Priority, ReporterIdDisplayName, AssigneeDisplayName, CreatedAt, UpdatedAt)
                VALUES
                (@Code, @Title, @Description, @Status, @DueDate, @Priority, @ReporterIdDisplayName, @AssigneeDisplayName, @CreatedAt, @UpdatedAt);
            ";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Code", task.Code ?? string.Empty);
            command.Parameters.AddWithValue("@Title", task.Title ?? string.Empty);
            command.Parameters.AddWithValue("@Description", task.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Status", task.Status);
            command.Parameters.AddWithValue("@DueDate", task.DueDate == DateTime.MinValue ? (object)DBNull.Value : task.DueDate);
            command.Parameters.AddWithValue("@Priority", task.Priority);
            command.Parameters.AddWithValue("@ReporterIdDisplayName", task.ReporterDisplayName ?? string.Empty);
            command.Parameters.AddWithValue("@AssigneeDisplayName", task.AssigneeDisplayName ?? string.Empty);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}
