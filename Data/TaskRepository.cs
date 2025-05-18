using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TaskManager.Contexts;
using TaskManager.Models;
using TaskManager.Services;

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

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = @"
                    SELECT T.Code, T.Title, T.Description, 
                           ISNULL(U.DisplayName, '(Unassigned)') AS Assignee, 
                           T.Status, T.DueDate, T.Priority, T.CreatedAt, T.UpdatedAt
                    FROM Tasks T
                    LEFT JOIN Users U ON T.AssigneeId = U.Id";

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
                Logger.Instance.Error($" [SQL] {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($" {ex.Message}");
            }

            return tasks;
        }

        public List<string> GetAllUsernamesFromDb()
        {
            var usernames = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = "SELECT UserName FROM Users WHERE IsActive = 1";
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    usernames.Add(reader["UserName"]?.ToString() ?? string.Empty);
                }
            }
            catch (SqlException ex)
            {
                Logger.Instance.Error($"[SQL] {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($" {ex.Message}");
            }

            return usernames;
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
