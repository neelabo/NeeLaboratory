using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeLaboratory.Windows.Data.Converters
{
    public class BooleanToOpacityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var boolean = (bool)value;
            return boolean ? 0.5 : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
