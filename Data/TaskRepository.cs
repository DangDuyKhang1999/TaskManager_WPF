using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TaskManager.Models;

namespace TaskManager.Data
{
    public class TaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TaskModel> GetAllTasks()
        {
            var tasks = new List<TaskModel>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
        SELECT T.Code, T.Title, T.Description, 
               ISNULL(U.DisplayName, N'(Chưa phân công)') AS Assignee, 
               T.Status, T.DueDate, T.Priority, T.CreatedAt, T.UpdatedAt
        FROM Tasks T
        LEFT JOIN Users U ON T.AssigneeId = U.Id";

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new TaskModel
                {
                    Id = reader["Code"].ToString(),
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"].ToString(),
                    Assignee = reader["Assignee"].ToString(),
                    Status = GetStatusString(Convert.ToInt32(reader["Status"])),
                    DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue,
                    Priority = Convert.ToInt32(reader["Priority"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                });
            }

            return tasks;
        }

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
