using System.Globalization;
using System.Windows.Data;

namespace No_Fast_No_Fun_Wpf.Converters
{
    public class MultiplyByFiveConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int intValue)
                return intValue * 5;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
