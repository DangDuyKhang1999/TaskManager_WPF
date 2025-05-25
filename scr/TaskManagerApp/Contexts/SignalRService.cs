using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows.Threading;
using TaskManagerApp.Services;
using TaskManagerApp.Common;

namespace TaskManagerApp.Contexts
{
    /// <summary>
    /// Singleton service that manages SignalR connection and real-time notifications
    /// for task and user changes.
    /// </summary>
    public class SignalRService
    {
        private HubConnection? _connection;

        /// <summary>
        /// Event triggered when a task change notification is received from the SignalR hub.
        /// </summary>
        public event Action? TasksChanged;

        /// <summary>
        /// Event triggered when a user change notification is received from the SignalR hub.
        /// </summary>
        public event Action? UsersChanged;

        private static readonly Lazy<SignalRService> _instance =
            new Lazy<SignalRService>(() => new SignalRService());

        /// <summary>
        /// Gets the singleton instance of the <see cref="SignalRService"/>.
        /// </summary>
        public static SignalRService Instance => _instance.Value;

        // Private constructor to enforce singleton pattern
        private SignalRService() { }

        /// <summary>
        /// Starts the SignalR connection to the specified hub URL.
        /// Registers handlers for TaskChanged and UserChanged events.
        /// </summary>
        /// <param name="hubUrl">The URL of the SignalR hub.</param>
        public async Task StartAsync(string hubUrl)
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On(AppConstants.SignalREvents.TaskChanged, () =>
            {
                // Ensure event is raised on UI thread
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    TasksChanged?.Invoke();
                    Logger.Instance.Information("SignalR event received: TaskChanged");
                });
            });

            _connection.On(AppConstants.SignalREvents.UserChanged, () =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    UsersChanged?.Invoke();
                    Logger.Instance.Information("SignalR event received: UserChanged");
                });
            });

            try
            {
                await _connection.StartAsync();
                Logger.Instance.Information("SignalR connection started.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SignalR connection error: " + ex.Message);
            }
        }

        /// <summary>
        /// Sends a task change notification to the SignalR hub.
        /// </summary>
        public async Task NotifyTaskChangedAsync()
        {
            if (_connection == null || _connection.State != HubConnectionState.Connected)
                return;

            try
            {
                await _connection.SendAsync(AppConstants.SignalREvents.NotifyTaskChanged);
                Logger.Instance.Information("SignalR notification sent: NotifyTaskChanged");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SignalR notify error: " + ex.Message);
            }
        }

        /// <summary>
        /// Sends a user change notification to the SignalR hub.
        /// </summary>
        public async Task NotifyUserChangedAsync()
        {
            if (_connection == null || _connection.State != HubConnectionState.Connected)
                return;

            try
            {
                await _connection.SendAsync(AppConstants.SignalREvents.NotifyUserChanged);
                Logger.Instance.Information("SignalR notification sent: NotifyUserChanged");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SignalR notify error: " + ex.Message);
            }
        }

        /// <summary>
        /// Stops and disposes the SignalR connection.
        /// </summary>
        public async Task StopAsync()
        {
            if (_connection != null)
            {
                try
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                    Logger.Instance.Information("SignalR connection stopped.");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("SignalR stop error: " + ex.Message);
                }
            }
        }
    }
}
