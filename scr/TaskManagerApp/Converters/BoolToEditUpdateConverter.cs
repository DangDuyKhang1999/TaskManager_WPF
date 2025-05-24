using System.Globalization;
using System.Windows.Data;

namespace TaskManagerApp.Converters
{
    public class BoolToEditUpdateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isEditing && isEditing ? "Update" : "Edit";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
