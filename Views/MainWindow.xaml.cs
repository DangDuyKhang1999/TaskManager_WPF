using System.Windows;
using TaskManager.Contexts;
using TaskManager.Services;

namespace TaskManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (UserSession.Instance.IsAdmin)
            {
                NewUserTab.Visibility = Visibility.Visible;
            }
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            Logger.Instance.Info("***** Task Manager end *****");
        }
    }
}
