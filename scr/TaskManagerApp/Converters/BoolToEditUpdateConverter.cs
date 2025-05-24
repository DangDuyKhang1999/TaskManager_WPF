using System.Globalization;
using System.Windows.Data;

namespace TaskManagerApp.Converters
{
    /// <summary>
    /// Converts a boolean value to a string: "Update" if true; otherwise "Edit".
    /// </summary>
    public class BoolToEditUpdateConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to the corresponding string.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use (not used).</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>"Update" if value is true; otherwise, "Edit".</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isEditing && isEditing ? "Update" : "Edit";
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>None.</returns>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
