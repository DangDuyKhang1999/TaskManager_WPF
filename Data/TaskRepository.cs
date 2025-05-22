using System.Data.SqlClient;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Models;
using TaskManager.Services;

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
                // Lấy EmployeeCode đầu tiên có IsAdmin == isAdmin (true hoặc false)
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

                if (isAdmin)
                {
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
                        u2.DisplayName AS AssigneeName
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
                        u2.DisplayName AS AssigneeName
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
                Logger.Instance.Information($"Production mode - User {employeeCode}, IsAdmin = {isAdmin}");

                string condition = isAdmin
                    ? "WHERE t.ReporterId = @EmployeeCode OR t.AssigneeId = @EmployeeCode"
                    : "WHERE t.AssigneeId = @EmployeeCode";

                string query = $@"
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
                    u2.DisplayName AS AssigneeName
                FROM Tasks t
                LEFT JOIN Users u1 ON t.ReporterId = u1.EmployeeCode
                LEFT JOIN Users u2 ON t.AssigneeId = u2.EmployeeCode
                {condition}";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(MapReaderToTask(reader));
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
    /// Maps the current record of a <see cref="SqlDataReader"/> to a <see cref="TaskModel"/> instance.
    /// </summary>
    /// <param name="reader">The SQL data reader positioned at a record.</param>
    /// <returns>A populated <see cref="TaskModel"/>.</returns>
    private TaskModel MapReaderToTask(SqlDataReader reader)
    {
        return new TaskModel
        {
            Id = Convert.ToInt32(reader["Id"]),
            Code = reader["Code"]?.ToString() ?? string.Empty,
            Title = reader["Title"]?.ToString() ?? string.Empty,
            Description = reader["Description"]?.ToString() ?? string.Empty,
            Status = Convert.ToInt32(reader["Status"]),
            DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue,
            Priority = Convert.ToInt32(reader["Priority"]),
            ReporterDisplayName = reader["ReporterName"]?.ToString() ?? "(Unknown)",
            AssigneeDisplayName = reader["AssigneeName"]?.ToString() ?? "(Unassigned)",
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
        };
    }
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

            Logger.Instance.DatabaseSuccess($"Task inserted successfully. Code: {task.Code}");
            return true;
        }
        catch (SqlException sqlEx)
        {
            int errorCode = sqlEx.Number;
            string errorMessage = $"SQL Error Code: {errorCode}, Message: {sqlEx.Message}{Environment.NewLine}{sqlEx.StackTrace}";

            Logger.Instance.DatabaseFailure(errorMessage);
            return false;
        }
        catch (Exception ex)
        {
            string errorMessage = $"General error: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
            Logger.Instance.DatabaseFailure(errorMessage);
            return false;
        }
    }
    public bool IsTaskCodeExists(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        using var connection = new SqlConnection(AppConstants.Database.ConnectionString);
        connection.Open();

        using var command = new SqlCommand("SELECT COUNT(1) FROM Tasks WHERE Code = @code", connection);
        command.Parameters.AddWithValue("@code", code);

        int count = (int)command.ExecuteScalar();
        return count > 0;
    }
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
                Logger.Instance.DatabaseSuccess($"Task with Code '{code}' deleted successfully.");
                return true;
            }
            else
            {
                Logger.Instance.Warning($"No task found with Code '{code}' to delete.");
                return false;
            }
        }
        catch (SqlException sqlEx)
        {
            int errorCode = sqlEx.Number;
            string errorMessage = $"SQL Error Code: {errorCode}, Message: {sqlEx.Message}{Environment.NewLine}{sqlEx.StackTrace}";

            Logger.Instance.DatabaseFailure(errorMessage);
            return false;
        }
        catch (Exception ex)
        {
            string errorMessage = $"General error: {ex.Message}{Environment.NewLine}{ex.StackTrace}";
            Logger.Instance.DatabaseFailure(errorMessage);
            return false;
        }
    }
}
