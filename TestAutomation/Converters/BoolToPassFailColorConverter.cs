using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TestAutomation.UI.Wpf.Converters;

public class BoolToPassFailColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Brushes.Green : Brushes.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}