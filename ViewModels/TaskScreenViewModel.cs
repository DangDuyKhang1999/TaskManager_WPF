using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;
using TaskManager.Common;
using TaskManager.Services;

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

        public ICommand DeleteTaskCommand { get; }

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

            DeleteTaskCommand = new RelayCommand(DeleteTask);
        }

        private void DeleteTask(object parameter)
        {
            if (parameter is not TaskModel task || string.IsNullOrWhiteSpace(task.Code))
                return;

            bool isDeleted = _taskRepository.DeleteTaskByCode(task.Code);

            if (isDeleted)
            {
                Tasks.Remove(task);
                Logger.Instance.Information($"Task '{task.Code}' removed from UI and database.");
            }
            else
            {
                Logger.Instance.Warning($"Failed to delete Task '{task.Code}'.");
            }
        }
    }
}
