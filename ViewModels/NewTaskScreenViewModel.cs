using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Data;

namespace TaskManager.ViewModels
{
    public class NewTaskScreenViewModel : INotifyPropertyChanged
    {
        private readonly UserRepository _userRepository;

        public ObservableCollection<string> ReporterUsers { get; }
        public ObservableCollection<string> AssigneeUsers { get; }

        private string _reporterId;
        public string ReporterId
        {
            get => _reporterId;
            set { _reporterId = value; OnPropertyChanged(); }
        }

        private string _assigneeId;
        public string AssigneeId
        {
            get => _assigneeId;
            set { _assigneeId = value; OnPropertyChanged(); }
        }

        public NewTaskScreenViewModel()
        {
            _userRepository = new UserRepository(AppConstants.Database.ConnectionString);

            // Load users from DB
            _userRepository.GetUsersAndAdmins(out var normalUsers, out var adminUsers);
            DatabaseContext.Instance.LoadNormalUsers(normalUsers);
            DatabaseContext.Instance.LoadAdminUsers(adminUsers);

            // Gán danh sách trực tiếp
            ReporterUsers = new ObservableCollection<string>(DatabaseContext.Instance.AdminUsersList);
            AssigneeUsers = new ObservableCollection<string>(DatabaseContext.Instance.NormalUsersList);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
