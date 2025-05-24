using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows.Threading;
using TaskManager.Services;

namespace TaskManagerApp.Contexts
{
    public class SignalRService
    {
        private HubConnection _connection;

        public event Action TasksChanged;

        private static readonly Lazy<SignalRService> _instance =
            new Lazy<SignalRService>(() => new SignalRService());

        public static SignalRService Instance => _instance.Value;

        private SignalRService() { }

        public async Task StartAsync(string hubUrl)
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On("TaskChanged", () =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    TasksChanged?.Invoke();
                    Logger.Instance.Information("SignalR event received: TaskChanged");
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

        public async Task NotifyTaskChangedAsync()
        {
            if (_connection == null || _connection.State != HubConnectionState.Connected)
                return;

            try
            {
                await _connection.SendAsync("NotifyTaskChanged");
                Logger.Instance.Information("SignalR notification sent: NotifyTaskChanged");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("SignalR notify error: " + ex.Message);
            }
        }

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
