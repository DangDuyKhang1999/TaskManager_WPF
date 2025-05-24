using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new task with validation support.
    /// </summary>
    public class NewTaskViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly UserRepository _userRepository;
        private bool _hasAttemptedSave;

        /// <summary>
        /// List of reporter user display names (admins).
        /// </summary>
        public ObservableCollection<string> ReporterUsers { get; }

        /// <summary>
        /// List of assignee user display names (normal users).
        /// </summary>
        public ObservableCollection<string> AssigneeUsers { get; }

        private string _reporterId;
        /// <summary>
        /// Selected reporter display name.
        /// </summary>
        public string ReporterDisplayName
        {
            get => _reporterId;
            set => SetProperty(ref _reporterId, value);
        }

        private string _assigneeId;
        /// <summary>
        /// Selected assignee display name.
        /// </summary>
        public string AssigneeDisplayName
        {
            get => _assigneeId;
            set => SetProperty(ref _assigneeId, value);
        }

        private string _code;
        /// <summary>
        /// Task code.
        /// </summary>
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        private string _title;
        /// <summary>
        /// Task title.
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description;
        /// <summary>
        /// Task description.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private int _status;
        /// <summary>
        /// Task status.
        /// </summary>
        public int Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private DateTime? _dueDate;
        /// <summary>
        /// Task due date.
        /// </summary>
        public DateTime? DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        private int _priority;
        /// <summary>
        /// Task priority.
        /// </summary>
        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        /// <summary>
        /// Command to save the task.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Command to clear all input fields.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewTaskViewModel"/> class.
        /// Loads users and initializes commands.
        /// </summary>
        public NewTaskViewModel()
        {
            _userRepository = new UserRepository(AppConstants.Database.ConnectionString);

            _userRepository.GetUsersAndAdmins(out var normalUsers, out var adminUsers);
            DatabaseContext.Instance.LoadNormalUsers(normalUsers);
            DatabaseContext.Instance.LoadAdminUsers(adminUsers);

            ReporterUsers = new ObservableCollection<string>(DatabaseContext.Instance.AdminUsersList);
            AssigneeUsers = new ObservableCollection<string>(DatabaseContext.Instance.NormalUsersList);

            SaveCommand = new RelayCommand(_ => SaveCommandExecute());
            ClearCommand = new RelayCommand(_ => ClearCommandExecute());

            ClearFields();
        }

        /// <summary>
        /// Provides validation error messages for properties.
        /// </summary>
        /// <param name="columnName">The property name.</param>
        /// <returns>Error message or null.</returns>
        public string this[string columnName]
        {
            get
            {
                if (!_hasAttemptedSave) return null;
                var taskRepository = new TaskRepository(AppConstants.Database.ConnectionString);
                switch (columnName)
                {
                    case nameof(Code):
                        if (string.IsNullOrWhiteSpace(Code))
                            return AppConstants.AppText.ValidationMessages.CodeRequired;
                        if (taskRepository.IsTaskCodeExists(Code))
                            return AppConstants.AppText.Message_TaskCodeExists;
                        break;
                    case nameof(Title):
                        if (string.IsNullOrWhiteSpace(Title))
                            return AppConstants.AppText.ValidationMessages.TitleRequired;
                        break;
                    case nameof(Status):
                        if (Status < 0 || Status > 2)
                            return AppConstants.AppText.ValidationMessages.InvalidStatus;
                        break;
                    case nameof(Priority):
                        if (Priority < 0 || Priority > 2)
                            return AppConstants.AppText.ValidationMessages.InvalidPriority;
                        break;
                    case nameof(ReporterDisplayName):
                        if (string.IsNullOrWhiteSpace(ReporterDisplayName))
                            return AppConstants.AppText.ValidationMessages.ReporterRequired;
                        break;
                    case nameof(AssigneeDisplayName):
                        if (string.IsNullOrWhiteSpace(AssigneeDisplayName))
                            return AppConstants.AppText.ValidationMessages.AssigneeRequired;
                        break;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets error message for the object. Always null.
        /// </summary>
        public string Error => null;

        /// <summary>
        /// Determines whether the current data is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (var prop in new[] { nameof(Code), nameof(Title), nameof(Status), nameof(Priority), nameof(ReporterDisplayName), nameof(AssigneeDisplayName) })
                {
                    if (this[prop] != null)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Executes the save command: validates, inserts task, logs, and notifies user.
        /// </summary>
        private void SaveCommandExecute()
        {
            _hasAttemptedSave = true;

            // Trigger validation for all relevant properties
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Priority));
            OnPropertyChanged(nameof(ReporterDisplayName));
            OnPropertyChanged(nameof(AssigneeDisplayName));

            if (!IsValid)
            {
                return;
            }

            // Retrieve employee codes by display names
            var reporterCode = GetEmployeeCodeByDisplayName(ReporterDisplayName);
            var assigneeCode = GetEmployeeCodeByDisplayName(AssigneeDisplayName);

            if (string.IsNullOrEmpty(reporterCode))
            {
                Logger.Instance.Warning($"{AppConstants.Database.ReporterCodeNotFound}{ReporterDisplayName}");
                return;
            }

            if (string.IsNullOrEmpty(assigneeCode))
            {
                Logger.Instance.Warning($"{AppConstants.Database.AssigneeCodeNotFound}{AssigneeDisplayName}");
                return;
            }

            var task = new TaskModel
            {
                Code = Code,
                Title = Title,
                Description = Description,
                Status = Status,
                Priority = Priority,
                DueDate = DueDate ?? DateTime.Now,
                ReporterId = reporterCode,
                AssigneeId = assigneeCode,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                var taskRepository = new TaskRepository(AppConstants.Database.ConnectionString);
                bool isInserted = taskRepository.InsertTask(task);
                if (isInserted)
                {
                    Logger.Instance.Information(
                      $"[New Task Created]\n" +
                      $"{AppConstants.Logging.BlankPadding}- Code: {task.Code}\n" +
                      $"{AppConstants.Logging.BlankPadding}- Title: {task.Title}\n" +
                      $"{AppConstants.Logging.BlankPadding}- Description: {task.Description}\n" +
                      $"{AppConstants.Logging.BlankPadding}- Status: {task.Status}\n" +
                      $"{AppConstants.Logging.BlankPadding}- Priority: {task.Priority}\n" +
                      $"{AppConstants.Logging.BlankPadding}- ReporterId (EmployeeCode): {task.ReporterId}\n" +
                      $"{AppConstants.Logging.BlankPadding}- AssigneeId (EmployeeCode): {task.AssigneeId}\n" +
                      $"{AppConstants.Logging.BlankPadding}- DueDate: {task.DueDate:yyyy-MM-dd}\n" +
                      $"{AppConstants.Logging.BlankPadding}- CreatedAt: {task.CreatedAt:yyyy-MM-dd HH:mm:ss}"
                    );

                    System.Windows.MessageBox.Show(
                        AppConstants.AppText.Message_TaskSaveSuccess,
                        AppConstants.ExecutionStatus.Success,
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        AppConstants.AppText.Message_TaskSaveFailed,
                        AppConstants.ExecutionStatus.Error,
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{AppConstants.AppText.Message_UnexpectedError}{ex.Message}");
            }
            ClearFields();
        }

        /// <summary>
        /// Executes the clear command to reset all fields.
        /// </summary>
        private void ClearCommandExecute()
        {
            ClearFields();
        }

        /// <summary>
        /// Resets all input fields and validation state.
        /// </summary>
        private void ClearFields()
        {
            _hasAttemptedSave = false;

            Code = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Status = -1;
            Priority = -1;
            DueDate = null;
            ReporterDisplayName = null;
            AssigneeDisplayName = null;

            // Notify UI to update bindings and clear validation errors
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Priority));
            OnPropertyChanged(nameof(ReporterDisplayName));
            OnPropertyChanged(nameof(AssigneeDisplayName));
        }

        /// <summary>
        /// Retrieves the employee code corresponding to a given display name.
        /// </summary>
        /// <param name="displayName">User's display name.</param>
        /// <returns>Employee code or null if not found.</returns>
        private string? GetEmployeeCodeByDisplayName(string displayName)
        {
            using var connection = new SqlConnection(AppConstants.Database.ConnectionString);
            connection.Open();

            using var command = new SqlCommand("SELECT EmployeeCode FROM Users WHERE DisplayName = @displayName", connection);
            command.Parameters.AddWithValue("@displayName", displayName);

            var result = command.ExecuteScalar();
            return result?.ToString();
        }
    }
}
