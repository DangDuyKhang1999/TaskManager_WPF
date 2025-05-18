using System;
using System.Data.SqlClient;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskService
    {
        private readonly string _connectionString;

        public TaskService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddTask(TaskModel task)
        {
            var query = @"
                INSERT INTO Tasks
                (Code, Title, Description, Status, DueDate, Priority, ReporterId, AssigneeId, CreatedAt, UpdatedAt)
                VALUES
                (@Code, @Title, @Description, @Status, @DueDate, @Priority, @ReporterId, @AssigneeId, @CreatedAt, @UpdatedAt);
            ";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Code", task.Code ?? "");
            command.Parameters.AddWithValue("@Title", task.Title ?? "");
            command.Parameters.AddWithValue("@Description", task.Description ?? "");
            command.Parameters.AddWithValue("@Status", task.Status);
            command.Parameters.AddWithValue("@DueDate", task.DueDate == DateTime.MinValue ? (object)DBNull.Value : task.DueDate);
            command.Parameters.AddWithValue("@Priority", task.Priority);
            command.Parameters.AddWithValue("@ReporterId", task.ReporterId ?? "");
            command.Parameters.AddWithValue("@AssigneeId", task.AssigneeId ?? "");
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}
