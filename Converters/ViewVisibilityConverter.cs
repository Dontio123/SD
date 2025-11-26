using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShelfMaster.Converters
{
    public class ViewVisibilityConverter : IValueConverter
    {
        // ConverterParameter should be the section name (string)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string? selectedSection = value as string;
            string? targetSection = parameter as string;

            if (selectedSection == null || targetSection == null)
                return Visibility.Collapsed;

            return string.Equals(selectedSection, targetSection, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

