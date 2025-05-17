using System;
using System.Windows;
using TaskManager.Views; // For Login Screen
using TaskManager.Services;
using TaskManager.Contexts;  // For Logger

namespace TaskManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Khởi tạo Logger (việc này sẽ tạo file log mới và bắt đầu ghi log)
            var logger = Logger.Instance;
            logger.Info("***** Task Manager start *****");
            // Tạo một cửa sổ dummy để ứng dụng không tự tắt khi LoginWindow đóng lại
            var dummyWindow = new Window
            {
                Title = "Task Manager"
            };
            dummyWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dummyWindow.Show();

            var loginWindow = new LoginWindow();
            var loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                Logger.Instance.Success($"Logged in successfully!!!");
                Logger.Instance.Info($"User: '{UserSession.Instance.UserName}', Admin: {UserSession.Instance.IsAdmin}");
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();

                // Đóng cửa sổ dummy sau khi mở MainWindow
                dummyWindow.Close();
            }
            else
            {
                logger.Info("User login failed or cancelled.");
                logger.Info("***** Task Manager end *****");
                Application.Current.Shutdown();
            }
        }
    }
}
