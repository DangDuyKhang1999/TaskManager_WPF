using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class NewTaskScreenViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;

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

            ClearFields(); // Reset các trường về mặc định khi khởi tạo ViewModel
        }

        private void SaveCommandExecute()
        {
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

            Logger.Instance.Info(
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
            Code = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Status = 0;
            Priority = 1;
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
