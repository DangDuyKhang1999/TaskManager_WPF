using System.Windows;
using TaskManager.Common;
using TaskManager.Contexts;
using TaskManager.Services;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TaskScreenViewModel();
            // Show the "New User" tab only if the current user is an admin
            if (UserSession.Instance.IsAdmin)
            {
                NewUserTab.Visibility = Visibility.Visible;
                UsersTab.Visibility = Visibility.Visible;
            }

            // Center the window on the screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Register event handler for window close event
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Handler for the window Closed event
        /// Logs the application shutdown info
        /// </summary>
        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            Logger.Instance.Information(AppConstants.Logging.TaskManagerEnd);
        }
    }
}
