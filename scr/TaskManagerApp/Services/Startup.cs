using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerSignalRHub.Hubs;

namespace TaskManagerApp.Services
{
    /// <summary>
    /// Configures SignalR services and request pipeline for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Adds SignalR services to the DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        /// <summary>
        /// Configures the HTTP request pipeline to route SignalR hubs.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Maps the TaskHub endpoint to "/taskhub".
                endpoints.MapHub<NotificationHub>("/taskhub");
            });
        }
    }
}
