using Microsoft.Data.SqlClient;
using TaskManagerApp.Common;
using TaskManagerApp.Contexts;
using TaskManagerApp.Models;
using TaskManagerApp.Services;
using System;

public class TaskRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of <see cref="TaskRepository"/> with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    public TaskRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Retrieves all tasks based on user role and application mode (debug or production).
    /// </summary>
    /// <returns>List of <see cref="TaskModel"/> objects.</returns>
    public List<TaskModel> GetAllTasks()
    {
        var tasks = new List<TaskModel>();

        try
        {
            Logger.Instance.Information("Opening database connection to load tasks.");
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            if (UserSession.Instance == null)
            {
                Logger.Instance.Error("UserSession is null. Cannot proceed.");
                return tasks;
            }

            string employeeCode = UserSession.Instance.EmployeeCode;
            bool isDebug = IniConfig.Mode?.ToLower() == "debug";
            bool isAdmin = UserSession.Instance.IsAdmin;

            if (isDebug)
            {
                try
                {
                    // In DEBUG mode, simulate behavior by selecting a debug employee code based on admin status
                    var userQuery = "SELECT TOP 1 EmployeeCode FROM Users WHERE IsAdmin = @IsAdmin ORDER BY EmployeeCode";
                    using var userCmd = new SqlCommand(userQuery, connection);
                    userCmd.Parameters.AddWithValue("@IsAdmin", isAdmin);
                    var debugEmployeeCode = userCmd.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(debugEmployeeCode))
                    {
                        Logger.Instance.Information($"DEBUG mode: Setting UserSession.EmployeeCode to '{debugEmployeeCode}' based on IsAdmin = {isAdmin}");
                        UserSession.Instance.SetEmployeeForDebug(debugEmployeeCode);
                        employeeCode = debugEmployeeCode;
                    }
                    else
                    {
                        Logger.Instance.Warning("DEBUG mode: No user found matching IsAdmin condition.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"DEBUG mode: Failed to simulate employee code. Error: {ex.Message}");
                }

                if (isAdmin)
                {
                    Logger.Instance.Information("DEBUG mode + Admin => Load ALL tasks.");
                    tasks.AddRange(LoadTasks(connection));
                }
                else
                {
                    Logger.Instance.Information("DEBUG mode + Non-admin => Load tasks assigned to the debug employee code.");
                    tasks.AddRange(LoadTasks(connection, employeeCode));
                }
            }
            else
            {
                Logger.Instance.Information($"Production mode - User {employeeCode}, IsAdmin = {isAdmin}");

                if (isAdmin)
                {
                    Logger.Instance.Information("Production mode + Admin => Load ALL tasks.");
                    tasks.AddRange(LoadTasks(connection));
                }
                else
                {
                    Logger.Instance.Information("Production mode + Non-admin => Load tasks assigned to the user's EmployeeCode.");
                    tasks.AddRange(LoadTasks(connection, employeeCode));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Failed to load tasks: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        return tasks;
    }

    /// <summary>
    /// Shared helper for loading tasks, optionally filtered by assignee.
    /// </summary>
    private List<TaskModel> LoadTasks(SqlConnection connection, string? assigneeCode = null)
    {
        var tasks = new List<TaskModel>();

        string query = @"
            SELECT 
                CAST(t.Id AS NVARCHAR) AS Id,
                t.Code,
                t.Title,
                t.Description,
                t.Status,
                t.DueDate,
                t.Priority,
                t.CreatedAt,
                t.UpdatedAt,
                u1.DisplayName AS ReporterName,
                u1.EmployeeCode AS ReporterId,
                u2.DisplayName AS AssigneeName,
                u2.EmployeeCode AS AssigneeId
            FROM Tasks t
            LEFT JOIN Users u1 ON t.ReporterId = u1.EmployeeCode
            LEFT JOIN Users u2 ON t.AssigneeId = u2.EmployeeCode";

        if (!string.IsNullOrWhiteSpace(assigneeCode))
        {
            query += " WHERE t.AssigneeId = @EmployeeCode";
        }

        using var command = new SqlCommand(query, connection);

        if (!string.IsNullOrWhiteSpace(assigneeCode))
        {
            command.Parameters.AddWithValue("@EmployeeCode", assigneeCode);
        }

        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error reading task records: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        return tasks;
    }

    /// <summary>
    /// Maps a data record from <see cref="SqlDataReader"/> to a <see cref="TaskModel"/>.
    /// </summary>
    private TaskModel MapReaderToTask(SqlDataReader reader)
    {
        try
        {
            return new TaskModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Code = reader["Code"]?.ToString() ?? string.Empty,
                Title = reader["Title"]?.ToString() ?? string.Empty,
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                Status = Convert.ToInt32(reader["Status"]),
                DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue,
                Priority = Convert.ToInt32(reader["Priority"]),
                ReporterDisplayName = reader["ReporterName"]?.ToString() ?? "(Unknown)",
                ReporterId = reader["ReporterId"] != DBNull.Value ? reader["ReporterId"].ToString() : null,
                AssigneeDisplayName = reader["AssigneeName"]?.ToString() ?? "(Unassigned)",
                AssigneeId = reader["AssigneeId"] != DBNull.Value ? reader["AssigneeId"].ToString() : null,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error mapping task record: {ex.Message}");
            return new TaskModel();
        }
    }

    public bool InsertTask(TaskModel task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
INSERT INTO Tasks
    (Code, Title, Description, Status, DueDate, Priority, ReporterId, AssigneeId, CreatedAt, UpdatedAt)
VALUES
    (@Code, @Title, @Description, @Status, @DueDate, @Priority, @ReporterId, @AssigneeId, GETDATE(), GETDATE())";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Code", task.Code ?? string.Empty);
            command.Parameters.AddWithValue("@Title", task.Title ?? string.Empty);
            command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(task.Description) ? DBNull.Value : (object)task.Description);
            command.Parameters.AddWithValue("@Status", task.Status);
            command.Parameters.AddWithValue("@DueDate", task.DueDate != DateTime.MinValue ? (object)task.DueDate : DBNull.Value);
            command.Parameters.AddWithValue("@Priority", task.Priority);
            command.Parameters.AddWithValue("@ReporterId", task.ReporterId ?? string.Empty);
            command.Parameters.AddWithValue("@AssigneeId", task.AssigneeId ?? string.Empty);

            command.ExecuteNonQuery();

            Logger.Instance.Information($"Task inserted successfully. Code: {task.Code}");
            return true;
        }
        catch (SqlException sqlEx)
        {
            Logger.Instance.Error($"SQL Error Code: {sqlEx.Number}, Message: {sqlEx.Message}{Environment.NewLine}{sqlEx.StackTrace}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"General error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            return false;
        }
    }

    public bool DeleteTaskByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "DELETE FROM Tasks WHERE Code = @Code";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Code", code);

            int affectedRows = command.ExecuteNonQuery();

            if (affectedRows > 0)
            {
                Logger.Instance.Information($"Task deleted successfully. Code: {code}");
                return true;
            }
            else
            {
                Logger.Instance.Error($"Delete failed. No task found with Code: {code}");
                return false;
            }
        }
        catch (SqlException sqlEx)
        {
            Logger.Instance.Error($"SQL Error Code: {sqlEx.Number}, Message: {sqlEx.Message}{Environment.NewLine}{sqlEx.StackTrace}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"General error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            return false;
        }
    }

    public bool IsTaskCodeExists(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand("SELECT COUNT(1) FROM Tasks WHERE Code = @code", connection);
            command.Parameters.AddWithValue("@code", code);

            var result = command.ExecuteScalar();
            int count = result != null && int.TryParse(result.ToString(), out int val) ? val : 0;
            return count > 0;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error checking task code existence: {ex.Message}");
            return false;
        }
    }

    public bool UpdateTask(TaskModel task)
    {
        if (task == null || string.IsNullOrWhiteSpace(task.Code))
            return false;

        try
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(@"
            UPDATE Tasks 
            SET 
                Title = @Title,
                Description = @Description,
                Status = @Status,
                DueDate = @DueDate,
                Priority = @Priority,
                ReporterId = @ReporterId,
                AssigneeId = @AssigneeId,
                UpdatedAt = GETDATE()
            WHERE Code = @Code", connection);

            command.Parameters.AddWithValue("@Title", task.Title ?? string.Empty);
            command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(task.Description) ? DBNull.Value : (object)task.Description);
            command.Parameters.AddWithValue("@Status", task.Status);
            command.Parameters.AddWithValue("@DueDate", task.DueDate != DateTime.MinValue ? (object)task.DueDate : DBNull.Value);
            command.Parameters.AddWithValue("@Priority", task.Priority);
            command.Parameters.AddWithValue("@ReporterId", task.ReporterId ?? string.Empty);
            command.Parameters.AddWithValue("@AssigneeId", task.AssigneeId ?? string.Empty);
            command.Parameters.AddWithValue("@Code", task.Code);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error updating task: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            return false;
        }
    }

    public bool UpdateTaskIdsFromDisplayNames(TaskModel task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string? assigneeId = null;
            string? reporterId = null;

            if (!string.IsNullOrWhiteSpace(task.AssigneeDisplayName))
            {
                using var cmd = new SqlCommand("SELECT TOP 1 EmployeeCode FROM Users WHERE DisplayName = @DisplayName", connection);
                cmd.Parameters.AddWithValue("@DisplayName", task.AssigneeDisplayName);
                assigneeId = cmd.ExecuteScalar()?.ToString();
            }

            if (!string.IsNullOrWhiteSpace(task.ReporterDisplayName))
            {
                using var cmd = new SqlCommand("SELECT TOP 1 EmployeeCode FROM Users WHERE DisplayName = @DisplayName", connection);
                cmd.Parameters.AddWithValue("@DisplayName", task.ReporterDisplayName);
                reporterId = cmd.ExecuteScalar()?.ToString();
            }

            if (string.IsNullOrEmpty(assigneeId) && string.IsNullOrEmpty(reporterId))
            {
                Logger.Instance.Warning($"No AssigneeId or ReporterId found from DisplayNames. Assignee='{task.AssigneeDisplayName}', Reporter='{task.ReporterDisplayName}'");
                return false;
            }

            if (!string.IsNullOrEmpty(assigneeId))
                task.AssigneeId = assigneeId;

            if (!string.IsNullOrEmpty(reporterId))
                task.ReporterId = reporterId;

            return true;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error updating IDs from DisplayNames: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            return false;
        }
    }
}
