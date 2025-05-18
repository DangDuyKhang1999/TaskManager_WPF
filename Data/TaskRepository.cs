using System.Data.SqlClient;
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

    /// <summary>
    /// Retrieves the employee code associated with a given username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The employee code if found; otherwise, an empty string.</returns>
    private string GetEmployeeCodeByUsername(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string query = "SELECT EmployeeCode FROM Users WHERE Username = @Username";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        var result = command.ExecuteScalar();
        return result?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Retrieves all tasks visible to the current user, filtered based on their role.
    /// Admin users see tasks where they are reporter or assignee.
    /// Non-admin users see tasks assigned only to them.
    /// </summary>
    /// <returns>A list of <see cref="TaskModel"/> objects representing the tasks.</returns>
    public List<TaskModel> GetAllTasks()
    {
        var tasks = new List<TaskModel>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string baseQuery = @"
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
            /**WHERE_CONDITION**/";  // placeholder to be replaced dynamically

            string whereCondition;
            string employeeCode = UserSession.Instance.EmployeeCode;

            if (UserSession.Instance.IsAdmin)
            {
                // Admin users see tasks where they are reporter or assignee
                whereCondition = "WHERE t.ReporterId = @EmployeeCode OR t.AssigneeId = @EmployeeCode";
            }
            else
            {
                // Non-admin users see tasks assigned to them only
                whereCondition = "WHERE t.AssigneeId = @EmployeeCode";
            }

            var query = baseQuery.Replace("/**WHERE_CONDITION**/", whereCondition);

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex.Message);
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
            Id = reader["Id"]?.ToString(),
            Code = reader["Code"]?.ToString() ?? string.Empty,
            Title = reader["Title"]?.ToString() ?? string.Empty,
            Description = reader["Description"]?.ToString() ?? string.Empty,
            Status = GetStatusString(Convert.ToInt32(reader["Status"])),
            DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue,
            Priority = GetPriorityString(Convert.ToInt32(reader["Priority"])),
            Reporter = reader["ReporterName"]?.ToString() ?? "(Unknown)",
            Assignee = reader["AssigneeName"]?.ToString() ?? "(Unassigned)",
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
        };
    }

    /// <summary>
    /// Converts an integer status code from the database into a user-friendly string representation for the UI.
    /// </summary>
    /// <param name="status">The status code stored as an integer in the database.</param>
    /// <returns>The corresponding status string to display in the UI.</returns>
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

    /// <summary>
    /// Converts an integer priority code from the database into a user-friendly string representation for the UI.
    /// </summary>
    /// <param name="priority">The priority code stored as an integer in the database.</param>
    /// <returns>The corresponding priority string to display in the UI.</returns>
    private string GetPriorityString(int priority)
    {
        return priority switch
        {
            0 => "High",
            1 => "Medium",
            2 => "Low",
            _ => "Unknown"
        };
    }
}
