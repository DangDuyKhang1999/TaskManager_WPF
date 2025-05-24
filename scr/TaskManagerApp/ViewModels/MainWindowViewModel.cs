using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// ViewModel for the main window, managing UI visibility and lifecycle events.
    /// </summary>
    public class MainWindowViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string WindowTitle => AppConstants.AppText.MainWindowTitle;

        /// <summary>
        /// Gets the visibility of the "New User" tab based on admin privileges.
        /// </summary>
        public Visibility NewUserTabVisibility =>
            UserSession.Instance?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility of the "Users" tab based on admin privileges.
        /// </summary>
        public Visibility UsersTabVisibility =>
            UserSession.Instance?.IsAdmin == true ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Command invoked when the main window is closed.
        /// </summary>
        public ICommand WindowClosedCommand { get; }

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
        {
            WindowClosedCommand = new RelayCommand(_ => OnWindowClosed());
            _ = InitializeSignalRAsync();
        }

        /// <summary>
        /// Initializes the SignalR client.
        /// This method currently runs synchronously without asynchronous operations.
        /// </summary>
        private Task InitializeSignalRAsync()
        {
            try
            {
                Logger.Instance.Information("SignalR client started.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SignalR connection error: " + ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles actions to perform when the main window is closed.
        /// </summary>
        public void OnWindowClosed()
        {
            Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
        }
    }
}
