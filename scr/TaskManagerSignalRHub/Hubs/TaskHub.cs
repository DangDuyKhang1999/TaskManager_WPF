using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TaskManagerSignalRHub.Hubs
{
    public class TaskHub : Hub
    {
        public async Task NotifyTaskChanged()
        {
            await Clients.Others.SendAsync("ReceiveTaskChanged");
        }
    }
}