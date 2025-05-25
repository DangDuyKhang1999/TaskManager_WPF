using Microsoft.Data.SqlClient;
using TaskManagerApp.Common;
using TaskManagerApp.Contexts;
using TaskManagerApp.Models;
using TaskManagerApp.Services;

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

            string employeeCode = UserSession.Instance.EmployeeCode;
            bool isDebug = IniConfig.Mode?.ToLower() == "debug";
            bool isAdmin = UserSession.Instance.IsAdmin;

            if (isDebug)
            {
                // In DEBUG mode, simulate behavior by selecting a debug employee code based on admin status
                var userQuery = "SELECT TOP 1 EmployeeCode FROM Users WHERE IsAdmin = @IsAdmin ORDER BY EmployeeCode";
                using var userCmd = new SqlCommand(userQuery, connection);
                userCmd.Parameters.AddWithValue("@IsAdmin", isAdmin);
                var debugEmployeeCode = userCmd.ExecuteScalar()?.ToString();

                if (!string.IsNullOrEmpty(debugEmployeeCode))
                {
                    // Override the session employee code for debug context
                    Logger.Instance.Information($"DEBUG mode: Setting UserSession.EmployeeCode to '{debugEmployeeCode}' based on IsAdmin = {isAdmin}");
                    UserSession.Instance.SetEmployeeForDebug(debugEmployeeCode);
                    employeeCode = debugEmployeeCode;
                }
                else
                {
                    Logger.Instance.Warning("DEBUG mode: No user found matching IsAdmin condition.");
                }

                if (isAdmin)
                {
                    // Admin in DEBUG mode retrieves all tasks
                    Logger.Instance.Information("DEBUG mode + Admin => Load ALL tasks.");
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

                    using var command = new SqlCommand(query, connection);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        tasks.Add(MapReaderToTask(reader));
                    }
                }
                else
                {
                    // Non-admin in DEBUG mode retrieves only their assigned tasks
                    Logger.Instance.Information("DEBUG mode + Non-admin => Load tasks assigned to the debug employee code.");

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
                LEFT JOIN Users u2 ON t.AssigneeId = u2.EmployeeCode
                WHERE t.AssigneeId = @EmployeeCode";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        tasks.Add(MapReaderToTask(reader));
                    }
                }
            }
            else
            {
                // In production mode, behavior depends on user's role
                Logger.Instance.Information($"Production mode - User {employeeCode}, IsAdmin = {isAdmin}");

                if (isAdmin)
                {
                    // Admin in production loads all tasks
                    Logger.Instance.Information("Production mode + Admin => Load ALL tasks.");
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

                    using var command = new SqlCommand(query, connection);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        tasks.Add(MapReaderToTask(reader));
                    }
                }
                else
                {
                    // Non-admin in production retrieves only their assigned tasks
                    Logger.Instance.Information("Production mode + Non-admin => Load tasks assigned to the user's EmployeeCode.");

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
                LEFT JOIN Users u2 ON t.AssigneeId = u2.EmployeeCode
                WHERE t.AssigneeId = @EmployeeCode";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        tasks.Add(MapReaderToTask(reader));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Failed to load tasks: {ex.Message}");
        }

        return tasks;
    }

    /// <summary>
    /// Maps a data record from <see cref="SqlDataReader"/> to a <see cref="TaskModel"/>.
    /// </summary>
    /// <param name="reader">The SQL data reader positioned at the current record.</param>
    /// <returns>A populated <see cref="TaskModel"/> instance.</returns>
    private TaskModel MapReaderToTask(SqlDataReader reader)
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

    /// <summary>
    /// Inserts a new task into the database.
    /// </summary>
    /// <param name="task">The task to insert.</param>
    /// <returns>True if insertion succeeds; otherwise false.</returns>
    public bool InsertTask(TaskModel task)
    {
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
            command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
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
            // Handle SQL-related errors
            int errorCode = sqlEx.Number;
            string errorMessage = $"SQL Error Code: {errorCode}, Message: {sqlEx.Message}{Environment.NewLine}{sqlEx.StackTrace}";

            Logger.Instance.Error(errorMessage);
            return false;
        }
        catch (Exception ex)
        {
            // Handle general errors
            string errorMessage = $"General error: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
            Logger.Instance.Error(errorMessage);
            return false;
        }
    }

    /// <summary>
    /// Deletes a task by its Code.
    /// </summary>
    /// <param name="code">The task code.</param>
    /// <returns>True if deletion succeeds; otherwise false.</returns>
    public bool DeleteTaskByCode(string code)
    {
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
            Logger.Instance.Error(
                $"SQL Error Code: {sqlEx.Number}, Message: {sqlEx.Message}{Environment.NewLine}{sqlEx.StackTrace}"
            );
            return false;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(
                $"General error: {ex.Message}{Environment.NewLine}{ex.StackTrace}"
            );
            return false;
        }
    }

    /// <summary>
    /// Checks if a task with the specified code exists in the database.
    /// </summary>
    /// <param name="code">The task code.</param>
    /// <returns>True if the code exists; otherwise false.</returns>
    public bool IsTaskCodeExists(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand("SELECT COUNT(1) FROM Tasks WHERE Code = @code", connection);
        command.Parameters.AddWithValue("@code", code);

        int count = (int)command.ExecuteScalar();
        return count > 0;
    }

    /// <summary>
    /// Updates an existing task in the database by task Code.
    /// </summary>
    /// <param name="task">The task to update.</param>
    /// <returns>True if update succeeds; otherwise false.</returns>
    public bool UpdateTask(TaskModel task)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
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
            command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
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
    }

    /// <summary>
    /// Updates the AssigneeId and ReporterId properties of a task using their corresponding DisplayNames.
    /// </summary>
    /// <param name="task">The task to update.</param>
    /// <returns>True if at least one ID was successfully updated; otherwise false.</returns>
    public bool UpdateTaskIdsFromDisplayNames(TaskModel task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string? assigneeId = null;
            string? reporterId = null;

            // Lookup AssigneeId from DisplayName
            if (!string.IsNullOrEmpty(task.AssigneeDisplayName))
            {
                string assigneeQuery = "SELECT TOP 1 EmployeeCode FROM Users WHERE DisplayName = @DisplayName";
                using var cmd = new SqlCommand(assigneeQuery, connection);
                cmd.Parameters.AddWithValue("@DisplayName", task.AssigneeDisplayName);
                var result = cmd.ExecuteScalar();
                assigneeId = result?.ToString();
            }

            // Lookup ReporterId from DisplayName
            if (!string.IsNullOrEmpty(task.ReporterDisplayName))
            {
                string reporterQuery = "SELECT TOP 1 EmployeeCode FROM Users WHERE DisplayName = @DisplayName";
                using var cmd = new SqlCommand(reporterQuery, connection);
                cmd.Parameters.AddWithValue("@DisplayName", task.ReporterDisplayName);
                var result = cmd.ExecuteScalar();
                reporterId = result?.ToString();
            }

            // Validate that at least one value was found
            if (string.IsNullOrEmpty(assigneeId) && string.IsNullOrEmpty(reporterId))
            {
                Logger.Instance.Warning($"No AssigneeId or ReporterId found from DisplayName. AssigneeDisplayName='{task.AssigneeDisplayName}', ReporterDisplayName='{task.ReporterDisplayName}'");
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
            Logger.Instance.Error($"Error updating IDs from DisplayNames: {ex.Message}");
            return false;
        }
    }
}