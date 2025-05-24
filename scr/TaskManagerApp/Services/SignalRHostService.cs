using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace TaskManagerApp.Services
{
    /// <summary>
    /// Hosts the SignalR server internally within the application.
    /// </summary>
    public class SignalRHostService
    {
        private IHost? _host;

        /// <summary>
        /// Starts the SignalR host server asynchronously on localhost:5000.
        /// </summary>
        public async Task StartAsync()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://localhost:5000");
                    webBuilder.UseStartup<Startup>();
                })
                .Build();

            await _host.StartAsync();
        }

        /// <summary>
        /// Stops the SignalR host server asynchronously and disposes resources.
        /// </summary>
        public async Task StopAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
    }
}
