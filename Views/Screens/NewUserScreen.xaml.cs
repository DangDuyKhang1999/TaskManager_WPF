using System.Windows.Controls;
using System.Windows;
using TaskManager.ViewModels;

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
                var passwordBox = (PasswordBox)sender;
                vm.Password = passwordBox.Password;

                // Trigger lại validation cho Password property qua binding ảo Tag
                var bindingExpression = passwordBox.GetBindingExpression(PasswordBox.TagProperty);
                bindingExpression?.UpdateSource();
            }
        }
    }
}
