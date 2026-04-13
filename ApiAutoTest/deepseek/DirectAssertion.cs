// DirectAssertion.cs
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class DirectAssertion : IAssertion
    {
        public AssertionBehavior Behavior { get; set; }
        public string Path { get; set; } = string.Empty; // JSONPath or XPath
        public Operator Operator { get; set; }
        public object? ExpectedValue { get; set; }

        public async Task<AssertionResult> AssertAsync(ComponentResult result, ExecutionContext context)
        {
            var actual = JsonPathEvaluator.Evaluate(result.Output, Path);
            bool passed = EvaluateOperator(actual, Operator, ExpectedValue);
            return new AssertionResult
            {
                Passed = passed,
                Behavior = Behavior,
                Actual = actual,
                Expected = ExpectedValue,
                Message = passed ? null : $"Expected {Operator} {ExpectedValue}, got {actual}"
            };
        }

        /// <summary>Shared evaluation logic, also used by VariableAssertion.</summary>
        internal static bool EvaluateOperator(object? actual, Operator op, object? expected)
        {
            return op switch
            {
                Operator.Equals => Equals(actual, expected),
                Operator.NotEquals => !Equals(actual, expected),
                Operator.Contains => actual?.ToString()?.Contains(expected?.ToString() ?? "") == true,
                Operator.MatchesRegex => Regex.IsMatch(actual?.ToString() ?? "", expected?.ToString() ?? ""),
                Operator.GreaterThan => Compare(actual, expected) > 0,
                Operator.LessThan => Compare(actual, expected) < 0,
                Operator.IsEmpty => actual == null || string.IsNullOrEmpty(actual.ToString()),
                Operator.IsNotEmpty => actual != null && !string.IsNullOrEmpty(actual.ToString()),
                _ => false
            };
        }

        private static int Compare(object? a, object? b)
        {
            if (a is IComparable ca && b is IComparable cb)
                return ca.CompareTo(cb);
            return 0;
        }
    }
}