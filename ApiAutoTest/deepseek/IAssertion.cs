// IAssertion.cs
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public interface IAssertion
    {
        AssertionBehavior Behavior { get; set; }
        string Path { get; set; }           // add
        Operator Operator { get; set; }     // add
        object? ExpectedValue { get; set; } // add
        Task<AssertionResult> AssertAsync(ComponentResult componentResult, ExecutionContext context);
    }
}