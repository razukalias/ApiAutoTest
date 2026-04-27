using System.Windows;
using TestAutomation.UI.Wpf.ViewModels;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void ProjectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.SelectedComponent = e.NewValue as ITestComponent;
            // For debugging, you can add:
            System.Diagnostics.Debug.WriteLine($"Selected: {vm.SelectedComponent?.Name}");
        }
    }

}