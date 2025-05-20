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
    public class NewTaskScreenViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly UserRepository _userRepository;
        private bool _hasAttemptedSave;

        public ObservableCollection<string> ReporterUsers { get; }
        public ObservableCollection<string> AssigneeUsers { get; }

        private string _reporterId;
        public string ReporterId
        {
            get => _reporterId;
            set => SetProperty(ref _reporterId, value);
        }

        private string _assigneeId;
        public string AssigneeId
        {
            get => _assigneeId;
            set => SetProperty(ref _assigneeId, value);
        }

        private string _code;
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private int _status;
        public int Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        private int _priority;
        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }

        public NewTaskScreenViewModel()
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

        public string this[string columnName]
        {
            get
            {
                if (!_hasAttemptedSave) return null;

                switch (columnName)
                {
                    case nameof(Code):
                        if (string.IsNullOrWhiteSpace(Code))
                            return AppConstants.AppText.ValidationMessages.CodeRequired;
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
                    case nameof(ReporterId):
                        if (string.IsNullOrWhiteSpace(ReporterId))
                            return AppConstants.AppText.ValidationMessages.ReporterRequired;
                        break;
                    case nameof(AssigneeId):
                        if (string.IsNullOrWhiteSpace(AssigneeId))
                            return AppConstants.AppText.ValidationMessages.AssigneeRequired;
                        break;
                }
                return null;
            }
        }

        public string Error => null;

        public bool IsValid
        {
            get
            {
                foreach (var prop in new[] { nameof(Code), nameof(Title), nameof(Status), nameof(Priority), nameof(ReporterId), nameof(AssigneeId) })
                {
                    if (this[prop] != null)
                        return false;
                }
                return true;
            }
        }

        private void SaveCommandExecute()
        {
            _hasAttemptedSave = true;

            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Priority));
            OnPropertyChanged(nameof(ReporterId));
            OnPropertyChanged(nameof(AssigneeId));

            if (!IsValid)
            {
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
                ReporterDisplayName = ReporterId,
                AssigneeDisplayName = AssigneeId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var reporterCode = GetEmployeeCodeByDisplayName(task.ReporterDisplayName);
            var assigneeCode = GetEmployeeCodeByDisplayName(task.AssigneeDisplayName);

            Logger.Instance.Information(
                $"[New Task Created]\n" +
                $"- Code: {task.Code}\n" +
                $"- Title: {task.Title}\n" +
                $"- Description: {task.Description}\n" +
                $"- Status: {task.StatusString}\n" +
                $"- Priority: {task.PriorityString}\n" +
                $"- Reporter (EmployeeCode): {reporterCode ?? "Unknown"}\n" +
                $"- Assignee (EmployeeCode): {assigneeCode ?? "Unknown"}\n" +
                $"- DueDate: {task.DueDate:yyyy-MM-dd}\n" +
                $"- CreatedAt: {task.CreatedAt:yyyy-MM-dd HH:mm:ss}"
            );
        }

        private void ClearCommandExecute()
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _hasAttemptedSave = false;

            Code = string.Empty;
            Title = string.Empty;
            Description = string.Empty;

            // Set Status and Priority to 3 to ensure no default selection in the ComboBoxes.
            // Since valid ComboBox values (Tags) are 0, 1, and 2,
            // 3 is outside this range and thus no item will be selected by default.
            Status = 3;
            Priority = 3;

            DueDate = null;
            ReporterId = null;
            AssigneeId = null;
        }

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
