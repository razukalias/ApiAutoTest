// AssertionResult.cs
namespace TestAutomationEngine.Core
{
    public class AssertionResult
    {
        public bool Passed { get; set; }
        public AssertionBehavior Behavior { get; set; }
        public string? Message { get; set; }
        public object? Actual { get; set; }
        public object? Expected { get; set; }
    }
}