using System.Globalization;
using System.Windows.Data;

namespace AutoLoggerV2.Converters
{
    public class ShowPasswordButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool showPassword)
            {
                return showPassword ? "숨김" : "확인";
            }
            return "Show";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
