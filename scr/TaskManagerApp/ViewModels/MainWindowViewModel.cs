using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Common;
using TaskManagerApp.Contexts;
using TaskManagerApp.Services;

namespace TaskManagerApp.ViewModels
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

        // Service to host the internal SignalR server.
        private readonly SignalRHostService _signalRHostService = new();

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// Sets up the window close command and starts the SignalR server.
        /// </summary>
        public MainWindowViewModel()
        {
            WindowClosedCommand = new RelayCommand(_ => OnWindowClosed());
            _ = InitializeSignalRAsync();
        }

        /// <summary>
        /// Starts the SignalR host server asynchronously and logs the result.
        /// </summary>
        private Task InitializeSignalRAsync()
        {
            try
            {
                _signalRHostService.StartAsync().ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Logger.Instance.Information("SignalR Hub server started.");
                    }
                    else if (t.IsFaulted && t.Exception != null)
                    {
                        Logger.Instance.Error("SignalR Hub start error: " + t.Exception.GetBaseException().Message);
                    }
                });

                Logger.Instance.Information("SignalR client started.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SignalR connection error: " + ex.Message);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the SignalR host server asynchronously and logs the result when the window is closed.
        /// </summary>
        public void OnWindowClosed()
        {
            try
            {
                _signalRHostService.StopAsync().ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
                    }
                    else if (t.IsFaulted && t.Exception != null)
                    {
                        Logger.Instance.Error("Error stopping SignalR Hub: " + t.Exception.GetBaseException().Message);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Error stopping SignalR Hub: " + ex.Message);
            }

            Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
        }
    }
}
