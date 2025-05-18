using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// Main ViewModel class that provides data binding for the main UI.
    /// It exposes task collection and available assignees for UI binding.
    /// </summary>
    public class MainViewModel
    {
        /// <summary>
        /// Collection of tasks displayed in the UI.
        /// ObservableCollection supports automatic UI updates when the collection changes.
        /// </summary>
        public ObservableCollection<TaskModel> Tasks { get; set; }

        /// <summary>
        /// List of available usernames for assigning tasks.
        /// Also ObservableCollection to update UI if the list changes.
        /// </summary>
        public ObservableCollection<string> AvailableAssignees { get; set; }

        // Repository instance to fetch data from database
        private readonly TaskRepository _repository;

        /// <summary>
        /// Constructor initializes the repository, loads tasks and assignees from the database,
        /// and populates the observable collections for data binding.
        /// </summary>
        public MainViewModel()
        {
            // Database connection string - adjust as needed
            string connectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";
            _repository = new TaskRepository(connectionString);

            // Load all tasks from the database
            var taskList = _repository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);

            // Load all active usernames into shared DatabaseContext singleton
            var allUsernames = _repository.GetAllUsernamesFromDb();
            DatabaseContext.Instance.LoadUserNames(allUsernames);

            // Use the loaded usernames as AvailableAssignees for UI binding
            AvailableAssignees = new ObservableCollection<string>(DatabaseContext.Instance.UserNames);
        }
    }
}
