using System.Globalization;
using System.Windows.Data;

namespace No_Fast_No_Fun_Wpf.Converters
{
    public class NullToBoolConverter : IValueConverter {
        // Retourne true si la valeur n'est pas null
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null;

        // Non supporté en remontée
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
