using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeLaboratory.Windows.Data.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public Visibility True { get; set; } = Visibility.Visible;
        public Visibility False { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (bool)value ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
