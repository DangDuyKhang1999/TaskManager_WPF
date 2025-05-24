using System.Windows;
using System.Windows.Controls;

namespace TaskManager.Services
{
    /// <summary>
    /// Provides attached properties to enable data binding for PasswordBox.Password.
    /// </summary>
    public static class PasswordBoxHelper
    {
        /// <summary>
        /// Attached DependencyProperty for binding the password string.
        /// </summary>
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        /// <summary>
        /// Gets the BoundPassword property value.
        /// </summary>
        public static string GetBoundPassword(DependencyObject obj) =>
            (string)obj.GetValue(BoundPasswordProperty);

        /// <summary>
        /// Sets the BoundPassword property value.
        /// </summary>
        public static void SetBoundPassword(DependencyObject obj, string value) =>
            obj.SetValue(BoundPasswordProperty, value);

        /// <summary>
        /// Attached DependencyProperty to enable or disable two-way binding for Password.
        /// </summary>
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        /// <summary>
        /// Gets the BindPassword property value.
        /// </summary>
        public static bool GetBindPassword(DependencyObject obj) =>
            (bool)obj.GetValue(BindPasswordProperty);

        /// <summary>
        /// Sets the BindPassword property value.
        /// </summary>
        public static void SetBindPassword(DependencyObject obj, bool value) =>
            obj.SetValue(BindPasswordProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // Avoid recursive update if value is already set
                if (passwordBox.Password != (string)e.NewValue)
                {
                    passwordBox.Password = (string)e.NewValue;
                }
            }
        }

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    // Attach event handler for two-way binding
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    // Detach event handler when two-way binding is disabled
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // Update BoundPassword attached property with current Password value
                SetBoundPassword(passwordBox, passwordBox.Password);
            }
        }
    }
}
