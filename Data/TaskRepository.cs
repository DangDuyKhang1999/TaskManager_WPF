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
                       T.Status
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
                    Status = GetStatusString(Convert.ToInt32(reader["Status"]))
                });
            }

            return tasks;
        }

        private string GetStatusString(int status)
        {
            return status switch
            {
                0 => "Chưa thực hiện",
                1 => "Đang thực hiện",
                2 => "Hoàn thành",
                _ => "Không xác định"
            };
        }
    }
}
