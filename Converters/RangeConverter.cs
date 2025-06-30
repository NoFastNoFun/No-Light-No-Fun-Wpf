using System.Globalization;
using System.Windows.Data;

namespace No_Fast_No_Fun_Wpf.Converters
{
    public class RangeConverter : IMultiValueConverter {
        // Concatène start et end en "start – end"
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length >= 2)
                return $"{values[0]} – {values[1]}";
            return string.Empty;
        }

        // Pas pris en charge dans ce scénario
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

