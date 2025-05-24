using System.Windows;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        // Tiêu đề của cửa sổ
        public string WindowTitle => AppConstants.AppText.MainWindowTitle;

        // Trạng thái hiển thị tab New User
        public Visibility NewUserTabVisibility =>
            UserSession.Instance?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

        // Trạng thái hiển thị tab Users
        public Visibility UsersTabVisibility =>
            UserSession.Instance?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

        // Xử lý sự kiện khi cửa sổ đóng
        public void OnWindowClosed()
        {
            Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
        }
    }
}
