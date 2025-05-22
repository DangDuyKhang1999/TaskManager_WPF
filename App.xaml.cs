using System;
using System.Windows;
using TaskManager.Views;      // For LoginWindow
using TaskManager.Services;   // For Logger
using TaskManager.Contexts;   // For UserSession
using TaskManager.Common;     // For AppConstants

namespace TaskManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize logger
            var logger = Logger.Instance;
            logger.Information(AppConstants.Logging.TaskManagerStart);

            bool skipLogin = false;

            if (IniConfig.Exists && IniConfig.Mode?.ToLower() == "debug")
            {
                string debugUsername = "debug_user";
                string employeeCode = "debug_user";
                bool isAdmin = IniConfig.IsAdmin;

                UserSession.Instance.Initialize(debugUsername, employeeCode, isAdmin);
                logger.Success("Auto-login in DEBUG mode");
                logger.Information($"User = '{debugUsername}', Admin = {isAdmin}");

                skipLogin = true;
            }

            // Dummy window to prevent app shutdown when dialog closes
            var dummyWindow = new Window
            {
                Title = AppConstants.AppText.MainWindowTitle,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            dummyWindow.Show();

            bool loginResult = false;

            if (skipLogin)
            {
                loginResult = true;
            }
            else
            {
                var loginWindow = new LoginWindow();
                loginResult = loginWindow.ShowDialog() == true;
            }

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
                logger.Information("User login failed or cancelled.");
                logger.Information("***** Task Manager end *****");
                logger.Shutdown(); // Ensure logger shuts down and flushes logs
                Application.Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Instance.Shutdown(); // Ensure all logs are written before exit
            base.OnExit(e);
        }
    }
}
