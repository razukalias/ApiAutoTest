using System.Windows;
using System.Windows.Controls;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.Views
{

    public class ComponentEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? HttpTemplate { get; set; }
        public DataTemplate? GraphQLTemplate { get; set; }
        public DataTemplate? LoopTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            System.Diagnostics.Debug.WriteLine($"SelectTemplate called with item: {item?.GetType().Name}");
            return item switch
            {
                HttpStep => HttpTemplate,
                GraphQLStep => GraphQLTemplate,
                Loop => LoopTemplate,
                _ => DefaultTemplate
            };
        }
    }
}