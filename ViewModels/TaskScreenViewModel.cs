using System.Collections.ObjectModel;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;
using TaskManager.Common;

namespace TaskManager.ViewModels
{
    public class TaskScreenViewModel : BaseViewModel
    {
        private ObservableCollection<TaskModel> _tasks;
        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        private ObservableCollection<string> _availableAssignees;
        public ObservableCollection<string> AvailableAssignees
        {
            get => _availableAssignees;
            set => SetProperty(ref _availableAssignees, value);
        }

        private readonly TaskRepository _taskRepository;
        private readonly UserRepository _userRepository;

        public TaskScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;

            _taskRepository = new TaskRepository(connectionString);
            _userRepository = new UserRepository(connectionString);

            var taskList = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(taskList);

            _userRepository.GetUsersAndAdmins(out var users, out var admins);

            DatabaseContext.Instance.LoadNormalUsers(users);
            DatabaseContext.Instance.LoadAdminUsers(admins);

            AvailableAssignees = new ObservableCollection<string>();
        }
    }
}
