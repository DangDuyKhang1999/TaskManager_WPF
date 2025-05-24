using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
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

        public ObservableCollection<KeyValuePair<int, string>> AvailableStatuses { get; }
            = new ObservableCollection<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(0, AppConstants.StatusValues.NotStarted),
                new KeyValuePair<int, string>(1, AppConstants.StatusValues.InProgress),
                new KeyValuePair<int, string>(2, AppConstants.StatusValues.Completed),
            };

        public ObservableCollection<KeyValuePair<int, string>> AvailablePriorities { get; }
            = new ObservableCollection<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(0, AppConstants.PriorityLevels.High),
                new KeyValuePair<int, string>(1, AppConstants.PriorityLevels.Medium),
                new KeyValuePair<int, string>(2, AppConstants.PriorityLevels.Low),
            };

        private readonly TaskRepository _taskRepository;
        private readonly UserRepository _userRepository;

        public ICommand DeleteTaskCommand { get; }
        public ICommand CellEditEndingCommand { get; }

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
            CellEditEndingCommand = new RelayCommand(OnCellEditEnding);
        }

        private void DeleteTask(object parameter)
        {
            if (parameter is not TaskModel task || string.IsNullOrWhiteSpace(task.Code))
                return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the task '{task.Title}'?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            bool isDeleted = _taskRepository.DeleteTaskByCode(task.Code);

            if (isDeleted)
            {
                Tasks.Remove(task);
                Logger.Instance.Information($"Task '{task.Code}' removed from UI and database.");
                System.Windows.MessageBox.Show(
                    $"Task '{task.Title}' was deleted successfully.",
                    "Success",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                Logger.Instance.Warning($"Failed to delete Task '{task.Code}'.");
                System.Windows.MessageBox.Show(
                    $"Failed to delete task '{task.Title}'. Please try again later.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void OnCellEditEnding(object parameter)
        {
            if (parameter is DataGridCellEditEndingEventArgs e)
            {
                if (e.Row.Item is TaskModel task)
                {
                    UpdateTask(task);
                }
            }
        }

        public void UpdateTask(TaskModel task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Code))
                return;

            bool isUpdated = _taskRepository.UpdateTask(task);

            if (isUpdated)
            {
                Logger.Instance.Information($"Task '{task.Code}' updated successfully.");
            }
            else
            {
                Logger.Instance.Warning($"Failed to update Task '{task.Code}'.");
                System.Windows.MessageBox.Show(
                    $"Failed to update task '{task.Title}'. Please try again later.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
