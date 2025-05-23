using System.Windows;
using System.Windows.Controls;

namespace TaskManager.Services
{
    public static class PasswordBoxHelper
    {
        // DependencyProperty để gắn Binding với PasswordBox.Password
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        // Hàm Get để lấy giá trị từ DependencyProperty
        public static string GetBoundPassword(DependencyObject obj) =>
            (string)obj.GetValue(BoundPasswordProperty);

        // Hàm Set để đặt giá trị cho DependencyProperty
        public static void SetBoundPassword(DependencyObject obj, string value) =>
            obj.SetValue(BoundPasswordProperty, value);

        // DependencyProperty để kiểm soát việc cập nhật hai chiều (TwoWay binding)
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static bool GetBindPassword(DependencyObject obj) =>
            (bool)obj.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject obj, bool value) =>
            obj.SetValue(BindPasswordProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // Ngăn việc lặp lại cập nhật nếu giá trị mới trùng với Password hiện tại
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
                    // Lắng nghe sự kiện PasswordChanged nếu được gắn TwoWay Binding
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // Lấy giá trị DependencyProperty BoundPassword và cập nhật
                SetBoundPassword(passwordBox, passwordBox.Password);
            }
        }
    }
}
