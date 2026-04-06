// AssertionBehavior.cs
namespace TestAutomationEngine.Core
{
    public enum AssertionBehavior
    {
        Fatal,   // Stop test plan on failure
        Assert,  // Mark failed but continue
        Warning  // Log warning only
    }

    public enum Operator
    {
        Equals, NotEquals, Contains, MatchesRegex, GreaterThan, LessThan, IsEmpty, IsNotEmpty
    }
}