using System;
using System.Globalization;
using System.Windows.Data;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.Converters;

public class AuthTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        return value.GetType(); // Return the Type object for ComboBoxItem.Tag comparison
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Type type)
        {
            if (type == typeof(ApiKeyStrategy)) return new ApiKeyStrategy();
            if (type == typeof(BearerTokenStrategy)) return new BearerTokenStrategy();
            if (type == typeof(OAuth2ClientCredentials)) return new OAuth2ClientCredentials();
            if (type == typeof(WindowsIntegratedStrategy)) return new WindowsIntegratedStrategy();
        }
        return null;
    }
}