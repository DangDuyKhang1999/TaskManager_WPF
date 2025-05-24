using System.Windows;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    /// <summary>
    /// Represents the login window for the application.
    /// Provides user authentication functionality and password handling.
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// Sets the startup location of the window and attaches event handlers for login success.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Attach a handler for login success if the DataContext is a LoginViewModel
            if (DataContext is LoginViewModel vm)
            {
                vm.LoginSucceeded += () =>
                {
                    this.DialogResult = true; // Indicate a successful login
                    this.Close(); // Close the login window
                };
            }
        }

        /// <summary>
        /// Handles the PasswordChanged event of the PasswordBox.
        /// Updates the ViewModel's Password property with the current password.
        /// </summary>
        /// <param name="sender">The source of the event (PasswordBox).</param>
        /// <param name="e">Event data for the RoutedEventArgs.</param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Ensure the DataContext is a LoginViewModel before updating the password
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }
}
