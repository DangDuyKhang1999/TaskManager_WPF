using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TaskManagerSignalRHub.Hubs
{
    /// <summary>
    /// SignalR hub to notify clients about task and user changes in real-time.
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Sends a notification to all connected clients except the caller
        /// that a task has changed, so clients can refresh or update task data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task NotifyTaskChanged()
        {
            await Clients.Others.SendAsync("TaskChanged");
        }

        /// <summary>
        /// Sends a notification to all connected clients except the caller
        /// that a user has changed, so clients can refresh or update user data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task NotifyUserChanged()
        {
            await Clients.Others.SendAsync("UserChanged");
        }
    }
}
