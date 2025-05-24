using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string WindowTitle => AppConstants.AppText.MainWindowTitle;

        public Visibility NewUserTabVisibility =>
            UserSession.Instance?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

        public Visibility UsersTabVisibility =>
            UserSession.Instance?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

        public ICommand WindowClosedCommand { get; }

        public MainWindowViewModel()
        {
            WindowClosedCommand = new RelayCommand(_ => OnWindowClosed());
            _ = InitializeSignalRAsync();
        }

        private async Task InitializeSignalRAsync()
        {
            try
            {
                Logger.Instance.Information("SignalR client started.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Lỗi kết nối SignalR: " + ex.Message);
            }
        }

        public void OnWindowClosed()
        {
            Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
        }
    }
}
