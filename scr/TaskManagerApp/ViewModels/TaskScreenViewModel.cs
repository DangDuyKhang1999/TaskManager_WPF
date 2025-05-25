using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Models;
using TaskManagerApp.Data;
using TaskManagerApp.Contexts;
using TaskManagerApp.Common;
using TaskManagerApp.Services;

namespace TaskManagerApp.ViewModels
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
        /// Collection of display names for reporters (admin users).
        /// </summary>
        public ObservableCollection<string> ReporterDisplayName { get; set; }

        /// <summary>
        /// Collection of display names for assignees (normal users).
        /// </summary>
        public ObservableCollection<string> AssigneesDisplayName { get; set; }

        /// <summary>
        /// Available status options mapped as key-value pairs (int ID to string name).
        /// </summary>
        public ObservableCollection<KeyValuePair<int, string>> AvailableStatuses { get; } =
            new()
            {
                new(0, AppConstants.StatusValues.NotStarted),
                new(1, AppConstants.StatusValues.InProgress),
                new(2, AppConstants.StatusValues.Completed),
            };

        /// <summary>
        /// Available priority options mapped as key-value pairs (int ID to string name).
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
        /// Command bound to task deletion action in the UI.
        /// </summary>
        public ICommand DeleteTaskCommand { get; }

        /// <summary>
        /// Command bound to task update action in the UI.
        /// </summary>
        public ICommand UpdateTaskCommand { get; }

        /// <summary>
        /// Constructor. Initializes repositories, loads data, registers event handlers, and starts SignalR connection.
        /// </summary>
        public TaskScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;

            _taskRepository = new TaskRepository(connectionString);
            _userRepository = new UserRepository(connectionString);

            // Load task list from database
            Tasks = new ObservableCollection<TaskModel>(_taskRepository.GetAllTasks());

            // Load user and admin lists for use in assigning/reporting tasks
            _userRepository.GetUsersAndAdmins(out var users, out var admins);
            DatabaseContext.Instance.LoadNormalUsers(users);
            DatabaseContext.Instance.LoadAdminUsers(admins);

            ReporterDisplayName = new ObservableCollection<string>(DatabaseContext.Instance.AdminUsersList);
            AssigneesDisplayName = new ObservableCollection<string>(DatabaseContext.Instance.NormalUsersList);

            DeleteTaskCommand = new RelayCommand(DeleteTask);
            UpdateTaskCommand = new RelayCommand(UpdateTaskFromButton);

            // Automatically reload tasks when a new task is saved
            TaskEvents.TaskSaved += () =>
            {
                Application.Current.Dispatcher.Invoke(ReloadTasks);
            };

            // Start SignalR connection for real-time updates
            _ = InitSignalRAsync();
        }

        /// <summary>
        /// Initializes SignalR connection and subscribes to task change events.
        /// </summary>
        private async Task InitSignalRAsync()
        {
            string hubUrl = "http://localhost:5000/taskhub";
            await SignalRService.Instance.StartAsync(hubUrl);
            SignalRService.Instance.TasksChanged += SignalR_OnTaskChanged;
        }

        /// <summary>
        /// Handler for task list changes received via SignalR.
        /// Triggers a task list reload on the UI thread.
        /// </summary>
        private void SignalR_OnTaskChanged()
        {
            Application.Current.Dispatcher.Invoke(ReloadTasks);
        }

        /// <summary>
        /// Reloads the task list from the database repository.
        /// </summary>
        private void ReloadTasks()
        {
            var tasksFromDb = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(tasksFromDb);
        }

        /// <summary>
        /// Deletes the specified task after confirmation from the user.
        /// </summary>
        /// <param name="parameter">The task to be deleted, passed as a TaskModel object.</param>
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
                Tasks.Remove(task); // Remove task from UI collection
                _ = SignalRService.Instance.NotifyTaskChangedAsync(); // Notify other clients
            }
            else
            {
                MessageBox.Show($"Failed to delete task '{task.Title}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the specified task after mapping display names to user IDs.
        /// </summary>
        /// <param name="task">The task to update.</param>
        public void UpdateTask(TaskModel task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Code)) return;

            // Ensure the Assignee and Reporter names are correctly mapped to their respective IDs
            bool idsUpdated = _taskRepository.UpdateTaskIdsFromDisplayNames(task);

            if (!idsUpdated)
            {
                MessageBox.Show($"Failed to map Assignee or Reporter for task '{task.Title}'.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isUpdated = _taskRepository.UpdateTask(task);

            if (isUpdated)
            {
                _ = SignalRService.Instance.NotifyTaskChangedAsync(); // Notify clients of change
            }
            else
            {
                MessageBox.Show($"Failed to update task '{task.Title}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Invoked from the UI to update a task after confirmation from the user.
        /// </summary>
        /// <param name="parameter">The task to update, passed as a TaskModel object.</param>
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
                UpdateTask(task); // Perform update logic
                MessageBox.Show($"Task '{task.Title}' updated successfully.", "Update Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
