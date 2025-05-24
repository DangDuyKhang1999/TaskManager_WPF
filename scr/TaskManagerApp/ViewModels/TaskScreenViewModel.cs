using System;
using System.Collections.ObjectModel;
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
    public class TaskScreenViewModel : BaseViewModel
    {
        private ObservableCollection<TaskModel> _tasks;
        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public ObservableCollection<string> AvailableAssignees { get; set; }

        public ObservableCollection<KeyValuePair<int, string>> AvailableStatuses { get; } =
            new ObservableCollection<KeyValuePair<int, string>>()
            {
                new(0, AppConstants.StatusValues.NotStarted),
                new(1, AppConstants.StatusValues.InProgress),
                new(2, AppConstants.StatusValues.Completed),
            };

        public ObservableCollection<KeyValuePair<int, string>> AvailablePriorities { get; } =
            new ObservableCollection<KeyValuePair<int, string>>()
            {
                new(0, AppConstants.PriorityLevels.High),
                new(1, AppConstants.PriorityLevels.Medium),
                new(2, AppConstants.PriorityLevels.Low),
            };

        private readonly TaskRepository _taskRepository;
        private readonly UserRepository _userRepository;

        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleEditCommand { get; }

        public TaskScreenViewModel()
        {
            string connectionString = AppConstants.Database.ConnectionString;

            _taskRepository = new TaskRepository(connectionString);
            _userRepository = new UserRepository(connectionString);

            Tasks = new ObservableCollection<TaskModel>(_taskRepository.GetAllTasks());

            foreach (var task in Tasks)
                task.IsEditing = false;

            _userRepository.GetUsersAndAdmins(out var users, out var admins);
            DatabaseContext.Instance.LoadNormalUsers(users);
            DatabaseContext.Instance.LoadAdminUsers(admins);

            AvailableAssignees = new ObservableCollection<string>();

            DeleteTaskCommand = new RelayCommand(DeleteTask);
            ToggleEditCommand = new RelayCommand(ToggleEdit);

            _ = InitSignalRAsync();
        }

        private async Task InitSignalRAsync()
        {
            string hubUrl = "http://localhost:5000/taskhub";

            await SignalRService.Instance.StartAsync(hubUrl);
            SignalRService.Instance.TasksChanged += SignalR_OnTaskChanged;
        }

        private void SignalR_OnTaskChanged()
        {
            Application.Current.Dispatcher.Invoke(ReloadTasks);
        }

        private void ReloadTasks()
        {
            var tasksFromDb = _taskRepository.GetAllTasks();
            Tasks = new ObservableCollection<TaskModel>(tasksFromDb);
        }

        private void DeleteTask(object parameter)
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

        public void UpdateTask(TaskModel task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Code)) return;

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

        private void ToggleEdit(object parameter)
        {
            if (parameter is not TaskModel task) return;

            if (task.IsEditing)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to update the task '{task.Title}'?",
                    "Confirm Update",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    UpdateTask(task);
                    task.IsEditing = false;
                    MessageBox.Show($"Task '{task.Title}' updated successfully.", "Update Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                { 
                    task.IsEditing = false;
                }
            }
            else
            {
                foreach (var t in Tasks)
                    t.IsEditing = false;

                task.IsEditing = true;
            }
        }
    }
}
