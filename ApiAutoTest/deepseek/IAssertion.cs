// IAssertion.cs
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public interface IAssertion
    {
        AssertionBehavior Behavior { get; }
        Task<AssertionResult> AssertAsync(ComponentResult componentResult, ExecutionContext context);
    }
}