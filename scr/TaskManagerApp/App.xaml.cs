using System;
using System.Windows;
using TaskManager.Views;      // For LoginWindow
using TaskManager.Services;   // For Logger & SignalRClientService
using TaskManager.Contexts;   // For UserSession
using TaskManager.Common;     // For AppConstants

namespace TaskManager
{
    public partial class App : Application
    {
        /// <summary>
        /// Override OnStartup to handle app initialization and login logic.
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize logger instance and log app start
            var logger = Logger.Instance;
            logger.Information(AppConstants.Logging.TaskManagerStart);

            bool skipLogin = false;

            // Check if running in debug mode to auto-login
            if (IniConfig.Exists && IniConfig.Mode?.ToLower() == "debug")
            {
                string debugUsername = "debug_user";
                string employeeCode = "debug_user";
                bool isAdmin = IniConfig.IsAdmin;

                // Initialize user session with debug credentials
                UserSession.Instance.Initialize(debugUsername, employeeCode, isAdmin);
                logger.Success("Auto-login in DEBUG mode");
                logger.Information($"User = '{debugUsername}', Admin = {isAdmin}");

                skipLogin = true;
            }

            // Create a dummy window to keep the app alive while login dialog is shown
            var dummyWindow = new Window
            {
                Title = AppConstants.AppText.MainWindowTitle,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            dummyWindow.Show();

            bool loginResult = false;

            // Show login window unless skipping login
            if (skipLogin)
            {
                loginResult = true;
            }
            else
            {
                var loginWindow = new LoginWindow();
                loginResult = loginWindow.ShowDialog() == true;
            }

            // If login successful, show main window and close dummy window
            if (loginResult)
            {
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();

                dummyWindow.Close();
            }
            else
            {
                // Log failure or cancellation and shutdown app
                logger.Information("User login failed or cancelled.");
                logger.Information("***** Task Manager end *****");
                logger.Shutdown(); // Flush logs before exit
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Override OnExit to ensure logger is properly shut down.
        /// </summary>
        /// <param name="e">Exit event arguments</param>
        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Instance.Shutdown(); // Flush and close logger on exit
            base.OnExit(e);
        }
    }
}
