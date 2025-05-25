using System;
using System.Windows;
using TaskManagerApp.Views;      // For LoginWindow
using TaskManagerApp.Services;   // For Logger & SignalRClientService
using TaskManagerApp.Contexts;   // For UserSession
using TaskManagerApp.Common;     // For AppConstants

namespace TaskManagerApp
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

            Window? dummyWindow = null;
            bool loginResult = false;

            if (!skipLogin)
            {
                dummyWindow = new Window
                {
                    Title = AppConstants.AppText.MainWindowTitle,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Width = 750,
                    Height = 800
                };
                dummyWindow.Show();

                var loginWindow = new LoginWindow();
                loginResult = loginWindow.ShowDialog() == true;
            }
            else
            {
                loginResult = true;
            }

            if (loginResult)
            {
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();

                dummyWindow?.Close();
            }
            else
            {
                logger.Information(AppConstants.Logging.LoginFailedCancel);
                logger.Information(AppConstants.Logging.TaskManagerEnd);
                logger.Shutdown();
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Override OnExit to ensure logger is properly shut down.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Instance.Shutdown();
            base.OnExit(e);
        }
    }
}
