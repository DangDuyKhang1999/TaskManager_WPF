using System;
using System.Windows;
using TaskManager.Views;      // For LoginWindow
using TaskManager.Services;   // For Logger
using TaskManager.Contexts;   // For UserSession
using TaskManager.Common;   // For Common Value

namespace TaskManager
{
    public partial class App : Application
    {
        /// <summary>
        /// Override OnStartup to handle login flow and main window initialization
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Logger - this creates a new log file and starts logging
            var logger = Logger.Instance;
            logger.Info(AppConstants.Logging.Message_TaskManagerStart);

            // Create a dummy window so the app doesn't shut down when LoginWindow closes
            var dummyWindow = new Window
            {
                Title = AppConstants.AppText.MainWindowTitle,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            dummyWindow.Show();

            // Show LoginWindow as a modal dialog
            var loginWindow = new LoginWindow();
            var loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                // Login successful, log info
                Logger.Instance.Success("Logged in successfully!!!");
                Logger.Instance.Info($"User = '{UserSession.Instance.UserName}', Admin = {UserSession.Instance.IsAdmin}");

                // Show main application window
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();

                // Close dummy window after main window is shown
                dummyWindow.Close();
            }
            else
            {
                // Login failed or cancelled, log and shutdown application
                logger.Info("User login failed or cancelled.");
                logger.Info("***** Task Manager end *****");
                Application.Current.Shutdown();
            }
        }
    }
}
