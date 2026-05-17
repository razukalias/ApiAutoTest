// File: TestAutomation/ViewModels/AssertionDisplayItem.cs (or TestAutomation/Models/AssertionDisplayItem.cs)
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.ViewModels
{
    public class AssertionDisplayItem
    {
        public IAssertion Assertion { get; }
        public string SourceDescription { get; set; }
        public string Path { get; set; }
        public string Operator { get; set; }
        public string ExpectedValue { get; set; }
        public string Behavior { get; set; }

        public AssertionDisplayItem(IAssertion assertion, string sourceDesc, string path, string op, string expected, string behavior)
        {
            Assertion = assertion;
            SourceDescription = sourceDesc;
            Path = path;
            Operator = op;
            ExpectedValue = expected;
            Behavior = behavior;
        }

        // Factory methods to create from existing assertions
        public static AssertionDisplayItem FromDirect(DirectAssertion d)
        {
            return new AssertionDisplayItem(d, "Response Body", d.Path, d.Operator.ToString(), d.ExpectedValue?.ToString() ?? "", d.Behavior.ToString());
        }

        public static AssertionDisplayItem FromVariable(VariableAssertion v)
        {
            return new AssertionDisplayItem(v, "Variable: " + v.Variable, v.Variable, v.Operator.ToString(), v.ExpectedValue?.ToString() ?? "", v.Behavior.ToString());
        }
    }
}