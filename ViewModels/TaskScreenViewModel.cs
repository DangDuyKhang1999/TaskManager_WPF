using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;
using TaskManager.Common;

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
        //private readonly UserRepository _userRepository;

        /// <summary>
        /// Constructor initializes repositories,
        /// loads tasks, users, and admins from the database,
        /// and initializes collections for UI binding.
        /// </summary>
        public TaskScreenViewModel()
        {
            _taskRepository = new TaskRepository(AppConstants.Database.ConnectionString);
            // Load tasks from database
            var taskList = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);
            AvailableAssignees = new ObservableCollection<string>();
        }
    }
}
