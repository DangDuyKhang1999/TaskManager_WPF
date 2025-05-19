using System.Collections.ObjectModel;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Data;

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

        public NewTaskScreenViewModel()
        {
            _userRepository = new UserRepository(AppConstants.Database.ConnectionString);

            _userRepository.GetUsersAndAdmins(out var normalUsers, out var adminUsers);
            DatabaseContext.Instance.LoadNormalUsers(normalUsers);
            DatabaseContext.Instance.LoadAdminUsers(adminUsers);

            ReporterUsers = new ObservableCollection<string>(DatabaseContext.Instance.AdminUsersList);
            AssigneeUsers = new ObservableCollection<string>(DatabaseContext.Instance.NormalUsersList);
        }
    }
}
