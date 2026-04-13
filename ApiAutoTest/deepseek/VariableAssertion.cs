// VariableAssertion.cs
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class VariableAssertion : IAssertion
    {
        public AssertionBehavior Behavior { get; set; }
        public string Variable { get; set; } = string.Empty;
        public Operator Operator { get; set; }
        public object? ExpectedValue { get; set; }

        public async Task<AssertionResult> AssertAsync(ComponentResult result, ExecutionContext context)
        {
            var actual = context.GetVariable(Variable);
            bool passed = DirectAssertion.EvaluateOperator(actual, Operator, ExpectedValue);
            return new AssertionResult
            {
                Passed = passed,
                Behavior = Behavior,
                Actual = actual,
                Expected = ExpectedValue,
                Message = passed ? null : $"Variable '{Variable}' {Operator} expected {ExpectedValue}, got {actual}"
            };
        }
    }
}