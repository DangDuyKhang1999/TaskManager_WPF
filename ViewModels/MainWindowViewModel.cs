using System.Windows;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string WindowTitle => AppConstants.AppText.MainWindowTitle;

        public Visibility NewUserTabVisibility =>
            UserSession.Instance.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        public void OnWindowClosed()
        {
            Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
        }
    }
}
