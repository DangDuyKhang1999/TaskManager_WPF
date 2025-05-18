using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// TaskScreenViewModel provides data binding for the main UI,
    /// including task list and available assignees (users/admins).
    /// </summary>
    public class TaskScreenViewModel
    {
        /// <summary>
        /// Collection of tasks to display in the UI.
        /// ObservableCollection notifies UI when items change.
        /// </summary>
        public ObservableCollection<TaskModel> Tasks { get; set; }

        /// <summary>
        /// Collection of assignees available for task assignment.
        /// This example does not set it here, but you can use users/admins separately.
        /// </summary>
        public ObservableCollection<string> AvailableAssignees { get; set; }

        // Repository for tasks data access
        private readonly TaskRepository _taskRepository;

        // Repository for user data access
        private readonly UserRepository _userRepository;

        /// <summary>
        /// Constructor initializes repositories,
        /// loads tasks, users, and admins from the database,
        /// and initializes collections for UI binding.
        /// </summary>
        public TaskScreenViewModel()
        {
            string connectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";

            _taskRepository = new TaskRepository(connectionString);
            _userRepository = new UserRepository(connectionString);

            // Load tasks from database
            var taskList = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);

            // Load users and admins from database
            _userRepository.GetUsersAndAdmins(out var users, out var admins);

            // Store users/admins in singleton context
            DatabaseContext.Instance.LoadNormalUsers(users);
            DatabaseContext.Instance.LoadAdminUsers(admins);

            // Initialize AvailableAssignees collection (to avoid CS8618 warning)
            AvailableAssignees = new ObservableCollection<string>();
        }
    }
}
