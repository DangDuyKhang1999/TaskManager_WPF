using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Data;
using TaskManager.Contexts;
using TaskManager.Common;
using TaskManager.Services;
using TaskManagerApp.Contexts;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// ViewModel for managing tasks on the Task screen.
    /// </summary>
    public class TaskScreenViewModel : BaseViewModel
    {
        private ObservableCollection<TaskModel>? _tasks;

        /// <summary>
        /// Collection of tasks displayed in the UI.
        /// </summary>
        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks ??= new ObservableCollection<TaskModel>();
            set => SetProperty(ref _tasks, value);
        }

        /// <summary>
        /// Collection of display names for reporters.
        /// </summary>
        public ObservableCollection<string> ReporterDisplayName { get; set; }

        /// <summary>
        /// Collection of display names for assignees.
        /// </summary>
        public ObservableCollection<string> AssigneesDisplayName { get; set; }

        /// <summary>
        /// Available status options for tasks.
        /// </summary>
        public ObservableCollection<KeyValuePair<int, string>> AvailableStatuses { get; } =
            new()
            {
                new(0, AppConstants.StatusValues.NotStarted),
                new(1, AppConstants.StatusValues.InProgress),
                new(2, AppConstants.StatusValues.Completed),
            };

        /// <summary>
        /// Available priority options for tasks.
        /// </summary>
        public ObservableCollection<KeyValuePair<int, string>> AvailablePriorities { get; } =
            new()
            {
                new(0, AppConstants.PriorityLevels.High),
                new(1, AppConstants.PriorityLevels.Medium),
                new(2, AppConstants.PriorityLevels.Low),
            };

        private readonly TaskRepository _taskRepository;
        private readonly UserRepository _userRepository;

        /// <summary>
        /// Command to delete a task.
        /// </summary>
        public ICommand DeleteTaskCommand { get; }

        /// <summary>
        /// Command to update a task.
        /// </summary>
        public ICommand UpdateTaskCommand { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TaskScreenViewModel"/>.
        /// </summary>
        public TaskScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;

            _taskRepository = new TaskRepository(connectionString);
            _userRepository = new UserRepository(connectionString);

            Tasks = new ObservableCollection<TaskModel>(_taskRepository.GetAllTasks());

            _userRepository.GetUsersAndAdmins(out var users, out var admins);
            DatabaseContext.Instance.LoadNormalUsers(users);
            DatabaseContext.Instance.LoadAdminUsers(admins);

            ReporterDisplayName = new ObservableCollection<string>(DatabaseContext.Instance.AdminUsersList);
            AssigneesDisplayName = new ObservableCollection<string>(DatabaseContext.Instance.NormalUsersList);

            DeleteTaskCommand = new RelayCommand(DeleteTask);
            UpdateTaskCommand = new RelayCommand(UpdateTaskFromButton);

            _ = InitSignalRAsync();
        }

        /// <summary>
        /// Initializes the SignalR connection and subscribes to task changes.
        /// </summary>
        private async Task InitSignalRAsync()
        {
            string hubUrl = "http://localhost:5000/taskhub";
            await SignalRService.Instance.StartAsync(hubUrl);
            SignalRService.Instance.TasksChanged += SignalR_OnTaskChanged;
        }

        /// <summary>
        /// Handler for SignalR task change events.
        /// </summary>
        private void SignalR_OnTaskChanged()
        {
            Application.Current.Dispatcher.Invoke(ReloadTasks);
        }

        /// <summary>
        /// Reloads tasks from the repository.
        /// </summary>
        private void ReloadTasks()
        {
            var tasksFromDb = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(tasksFromDb);
        }

        /// <summary>
        /// Deletes a task after user confirmation.
        /// </summary>
        /// <param name="parameter">The task to delete.</param>
        private void DeleteTask(object? parameter)
        {
            if (parameter is not TaskModel task || string.IsNullOrWhiteSpace(task.Code)) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the task '{task.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            bool isDeleted = _taskRepository.DeleteTaskByCode(task.Code);

            if (isDeleted)
            {
                Tasks.Remove(task);
                _ = SignalRService.Instance.NotifyTaskChangedAsync();
            }
            else
            {
                MessageBox.Show($"Failed to delete task '{task.Title}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates a task in the repository after mapping display names to IDs.
        /// </summary>
        /// <param name="task">The task to update.</param>
        public void UpdateTask(TaskModel task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Code)) return;

            bool idsUpdated = _taskRepository.UpdateTaskIdsFromDisplayNames(task);

            if (!idsUpdated)
            {
                MessageBox.Show($"Failed to map Assignee or Reporter for task '{task.Title}'.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isUpdated = _taskRepository.UpdateTask(task);

            if (isUpdated)
            {
                _ = SignalRService.Instance.NotifyTaskChangedAsync();
            }
            else
            {
                MessageBox.Show($"Failed to update task '{task.Title}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles update command triggered from UI with confirmation.
        /// </summary>
        /// <param name="parameter">The task to update.</param>
        private void UpdateTaskFromButton(object? parameter)
        {
            if (parameter is not TaskModel task) return;

            var result = MessageBox.Show(
                $"Are you sure you want to update the task '{task.Title}'?",
                "Confirm Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                UpdateTask(task);
                MessageBox.Show($"Task '{task.Title}' updated successfully.", "Update Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
