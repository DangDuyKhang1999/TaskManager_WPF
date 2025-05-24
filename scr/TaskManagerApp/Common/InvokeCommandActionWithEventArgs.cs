using System;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows;
namespace TaskManager.Common
{
    public class InvokeCommandActionWithEventArgs : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(InvokeCommandActionWithEventArgs));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(InvokeCommandActionWithEventArgs));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            var command = Command;
            var commandParameter = CommandParameter ?? parameter;

            if (command != null && command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }
}
