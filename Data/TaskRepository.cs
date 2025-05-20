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
        Logger.Instance.Info("TaskRepository initialized with connection string.");
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
            Logger.Instance.Info("Opening database connection to load tasks.");
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
            /**WHERE_CONDITION**/";

            string whereCondition;
            string employeeCode = UserSession.Instance.EmployeeCode;

            if (UserSession.Instance.IsAdmin)
            {
                Logger.Instance.Info($"User {employeeCode} is admin, loading tasks as reporter and assignee.");
                whereCondition = "WHERE t.ReporterId = @EmployeeCode OR t.AssigneeId = @EmployeeCode";
            }
            else
            {
                Logger.Instance.Info($"User {employeeCode} is not admin, loading tasks assigned to user.");
                whereCondition = "WHERE t.AssigneeId = @EmployeeCode";
            }

            var query = baseQuery.Replace("/**WHERE_CONDITION**/", whereCondition);

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

            Logger.Instance.Info("Executing task query.");
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapReaderToTask(reader));
            }

            Logger.Instance.Info($"Successfully loaded tasks for user {employeeCode}. Total tasks: {tasks.Count}.");
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
    public void InsertTask(TaskModel task)
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
            command.Parameters.AddWithValue("@ReporterId", task.ReporterDisplayName ?? string.Empty);
            command.Parameters.AddWithValue("@AssigneeId", task.AssigneeDisplayName ?? string.Empty);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Failed to insert task: {ex.Message}");
            throw; 
        }
    }

}
