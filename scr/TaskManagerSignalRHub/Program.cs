using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagerSignalRHub.Hubs;

/// <summary>
/// Configures and runs the SignalR Hub application.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Register SignalR services
builder.Services.AddSignalR();

var app = builder.Build();

// Map the TaskHub endpoint at /taskhub
app.MapHub<NotificationHub>("/taskhub");

// Start the application and listen for incoming requests
app.Run();
