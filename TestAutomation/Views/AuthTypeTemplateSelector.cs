using System.Windows;
using System.Windows.Controls;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.Views.Editors;

public class AuthTypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NoneTemplate { get; set; }
    public DataTemplate? ApiKeyTemplate { get; set; }
    public DataTemplate? BearerTokenTemplate { get; set; }
    public DataTemplate? OAuth2ClientCredentialsTemplate { get; set; }
    public DataTemplate? WindowsIntegratedTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            null => NoneTemplate,
            ApiKeyStrategy => ApiKeyTemplate,
            BearerTokenStrategy => BearerTokenTemplate,
            OAuth2ClientCredentials => OAuth2ClientCredentialsTemplate,
            WindowsIntegratedStrategy => WindowsIntegratedTemplate,
            _ => NoneTemplate
        };
    }
}