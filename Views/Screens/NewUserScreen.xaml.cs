using System.Windows.Controls;
using System.Windows;
using TaskManager.ViewModels;
using System.Windows.Data;

namespace TaskManager.Views.Screens
{
    public partial class NewUserScreen : UserControl
    {
        public NewUserScreen()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewUserViewModel vm)
            {
                var passwordBox = sender as PasswordBox;
                if (passwordBox != null)
                {
                    vm.Password = passwordBox.Password;

                    // Trigger validation binding
                    BindingExpression bindingExpression = passwordBox.GetBindingExpression(PasswordBox.TagProperty);
                    bindingExpression?.UpdateSource();
                }
            }
        }
    }
}
