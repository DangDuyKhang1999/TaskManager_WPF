using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;

namespace TaskManager.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<TaskModel> Tasks { get; set; }
        public ObservableCollection<string> AvailableAssignees { get; set; }

        private readonly TaskRepository _repository;

        public MainViewModel()
        {
            string connectionString = @"Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;";
            _repository = new TaskRepository(connectionString);

            // Load tasks from DB
            var taskList = _repository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);

            // Load all usernames into shared DatabaseContext
            var allUsernames = _repository.GetAllUsernamesFromDb();
            DatabaseContext.Instance.LoadUserNames(allUsernames);
            // Use loaded usernames for AvailableAssignees
            AvailableAssignees = new ObservableCollection<string>(DatabaseContext.Instance.UserNames);
        }
    }
}
