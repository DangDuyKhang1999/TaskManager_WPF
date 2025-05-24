using System;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace TaskManager.Common
{
    /// <summary>
    /// A behavior that allows an ICommand to be invoked with event arguments as a parameter.
    /// </summary>
    public class InvokeCommandActionWithEventArgs : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// Identifies the Command dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(InvokeCommandActionWithEventArgs));

        /// <summary>
        /// Gets or sets the command to be executed when the action is triggered.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Identifies the CommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(InvokeCommandActionWithEventArgs));

        /// <summary>
        /// Gets or sets the parameter to pass to the command.
        /// If not set, the event arguments are passed instead.
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Invokes the command with the provided parameter or event arguments.
        /// </summary>
        /// <param name="parameter">The event arguments passed by the trigger.</param>
        protected override void Invoke(object parameter)
        {
            var command = Command;
            var commandParameter = CommandParameter ?? parameter;

            if (command != null && command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }
}
