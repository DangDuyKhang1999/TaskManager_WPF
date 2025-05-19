using System.Windows;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (DataContext is LoginViewModel vm)
            {
                vm.LoginSucceeded += () =>
                {
                    this.DialogResult = true;
                    this.Close();
                };
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }
}
