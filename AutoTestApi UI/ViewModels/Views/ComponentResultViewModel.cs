using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.ViewModels;

public partial class ComponentResultViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _componentType = string.Empty;
    [ObservableProperty] private bool _success;
    [ObservableProperty] private long _durationMs;
    [ObservableProperty] private string _statusIcon = "⏳";
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private ObservableCollection<AssertionResultViewModel> _assertionResults = new();
    [ObservableProperty] private ObservableCollection<ComponentResultViewModel> _children = new();

    public ComponentResultViewModel() { }

    public static ComponentResultViewModel FromComponentResult(ComponentResult result)
    {
        var vm = new ComponentResultViewModel
        {
            Name = result.Component.Name,
            ComponentType = result.Component.ComponentType,
            Success = result.Success,
            DurationMs = result.DurationMs,
            StatusIcon = GetStatusIcon(result),
            HasError = result.HasUnhandledException || result.AssertionResults.Any(a => !a.Passed),
            ErrorMessage = result.Exception?.Message ?? string.Join("\n", result.AssertionResults.Where(a => !a.Passed).Select(a => a.Message))
        };

        foreach (var ar in result.AssertionResults)
            vm.AssertionResults.Add(new AssertionResultViewModel(ar));

        // Recursively process child results if the component is a container
        // Note: The core currently doesn't store child results in a hierarchical ComponentResult.
        // We'll need to extend core or build hierarchy from the test plan execution.
        // For simplicity, we'll handle only the root plan results for now.
        return vm;
    }

    private static string GetStatusIcon(ComponentResult result)
    {
        if (result.HasUnhandledException) return "❌";
        if (result.AssertionResults.Any(a => !a.Passed && a.Behavior == AssertionBehavior.Fatal)) return "❌";
        if (result.AssertionResults.Any(a => !a.Passed && a.Behavior == AssertionBehavior.Assert)) return "⚠️";
        if (result.AssertionResults.Any(a => !a.Passed && a.Behavior == AssertionBehavior.Warning)) return "⚠️";
        return "✅";
    }
}

public class AssertionResultViewModel
{
    public bool Passed { get; }
    public string Message { get; }
    public string Expected { get; }
    public string Actual { get; }

    public AssertionResultViewModel(AssertionResult ar)
    {
        Passed = ar.Passed;
        Message = ar.Message ?? string.Empty;
        Expected = ar.Expected?.ToString() ?? string.Empty;
        Actual = ar.Actual?.ToString() ?? string.Empty;
    }
}