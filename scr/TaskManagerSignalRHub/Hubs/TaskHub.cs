using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TaskManagerSignalRHub.Hubs
{
    /// <summary>
    /// SignalR hub to notify clients about task changes in real-time.
    /// </summary>
    public class TaskHub : Hub
    {
        /// <summary>
        /// Notify all connected clients except the caller that a task has changed.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task NotifyTaskChanged()
        {
            await Clients.Others.SendAsync("TaskChanged");
        }
    }
}
