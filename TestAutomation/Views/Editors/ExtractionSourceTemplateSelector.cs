using System.Windows;
using System.Windows.Controls;

namespace TestAutomation.UI.Wpf.Views.Editors;

public class ExtractionSourceTemplateSelector : DataTemplateSelector
{
    public DataTemplate? JsonTemplate { get; set; }
    public DataTemplate? HeadersTemplate { get; set; }
    public DataTemplate? PlainTextTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is string sourceType)
        {
            return sourceType switch
            {
                "Response Body" or "Request Body" => JsonTemplate,  // will be JSON if possible
                "Response Headers" => HeadersTemplate,
                "Status Code" => PlainTextTemplate,
                _ => null
            };
        }
        return null;
    }
}