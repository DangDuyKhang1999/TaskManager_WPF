using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TaskManager.ViewModels
{
    /// <summary>
    /// Base class for ViewModels implementing INotifyPropertyChanged.
    /// Provides property change notification support.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property. Automatically provided by compiler if omitted.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));


        /// <summary>
        /// Sets the field to the specified value and raises PropertyChanged event if the value changed.
        /// </summary>
        /// <typeparam name="T">Type of the field.</typeparam>
        /// <param name="field">Reference to the field.</param>
        /// <param name="value">New value to set.</param>
        /// <param name="propertyName">Name of the property. Automatically provided by compiler if omitted.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}
