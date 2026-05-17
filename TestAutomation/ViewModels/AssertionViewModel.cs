// File: TestAutomation/ViewModels/AssertionViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Bibliography;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.ViewModels;

public partial class AssertionViewModel : ObservableObject
{
    public IAssertion UnderlyingAssertion { get; private set; }

    [ObservableProperty]
    private string _sourceType = "Response Body";

    [ObservableProperty]
    private string _path = "";

    [ObservableProperty]
    private Operator _operator = Operator.Equals;

    [ObservableProperty]
    private string _expectedValue = "";

    [ObservableProperty]
    private AssertionBehavior _behavior = AssertionBehavior.Assert;

    public AssertionViewModel()
    {
        CreateUnderlyingAssertion();
    }

    public AssertionViewModel(IAssertion existing)
    {
        if (existing is DirectAssertion direct)
        {
            SourceType = "Response Body"; // can't distinguish from variable, default to body
            Path = direct.Path;
            Operator = direct.Operator;
            ExpectedValue = direct.ExpectedValue?.ToString() ?? "";
            Behavior = direct.Behavior;
        }
        else if (existing is VariableAssertion variable)
        {
            SourceType = "Existing Variable";
            Path = variable.Variable;
            Operator = variable.Operator;
            ExpectedValue = variable.ExpectedValue?.ToString() ?? "";
            Behavior = variable.Behavior;
        }
        UnderlyingAssertion = existing;
        // Subscribe to future property changes so they update the underlying object
        PropertyChanged += (s, e) => UpdateUnderlyingAssertion();
    }

    partial void OnSourceTypeChanged(string value)
    {
        // Swap underlying type when source changes
        CreateUnderlyingAssertion();
    }

    partial void OnPathChanged(string value) => UpdateUnderlyingAssertion();
    partial void OnOperatorChanged(Operator value) => UpdateUnderlyingAssertion();
    partial void OnExpectedValueChanged(string value) => UpdateUnderlyingAssertion();
    partial void OnBehaviorChanged(AssertionBehavior value) => UpdateUnderlyingAssertion();

    private void CreateUnderlyingAssertion()
    {
        if (SourceType == "Response Body" || SourceType == "Request Body")
        {
            UnderlyingAssertion = new DirectAssertion
            {
                Path = Path,
                Operator = Operator,
                ExpectedValue = ExpectedValue,
                Behavior = Behavior
            };
        }
        else  // Headers, Status Code, Existing Variable → VariableAssertion
        {
            // For headers/status code we store the variable name in Path
            UnderlyingAssertion = new VariableAssertion
            {
                Variable = Path,
                Operator = Operator,
                ExpectedValue = ExpectedValue,
                Behavior = Behavior
            };
        }
    }

    private void UpdateUnderlyingAssertion()
    {
        if (UnderlyingAssertion is DirectAssertion direct)
        {
            direct.Path = Path;
            direct.Operator = Operator;
            direct.ExpectedValue = ExpectedValue;
            direct.Behavior = Behavior;
        }
        else if (UnderlyingAssertion is VariableAssertion variable)
        {
            variable.Variable = Path;
            variable.Operator = Operator;
            variable.ExpectedValue = ExpectedValue;
            variable.Behavior = Behavior;
        }
    }
}