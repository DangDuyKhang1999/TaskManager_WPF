using System;
using System.Windows;
using TaskManager.Views;

namespace TaskManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Tạo một cửa sổ dummy để ứng dụng không tự tắt khi LoginWindow đóng lại
            var dummyWindow = new Window
            {
                Title = "Task Manager"
            };
            dummyWindow.Show();

            var loginWindow = new LoginWindow();
            var loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.Show();
                // Đóng cửa sổ dummy sau khi mở MainWindow
                dummyWindow.Close();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
