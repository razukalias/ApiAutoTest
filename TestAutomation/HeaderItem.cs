using CommunityToolkit.Mvvm.ComponentModel;

namespace TestAutomation.UI.Wpf;

public partial class HeaderItem : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

}