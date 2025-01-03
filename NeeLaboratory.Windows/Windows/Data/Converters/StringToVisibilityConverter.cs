using System;
using System.Windows;
using System.Windows.Data;

namespace NeeLaboratory.Windows.Data.Converters
{
    // 文字列状態を処理中表示に変換する
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public Visibility Null { get; set; } = Visibility.Collapsed;
        public Visibility NotNull { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = (string)value;
            return string.IsNullOrEmpty(s) ? Null : NotNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
