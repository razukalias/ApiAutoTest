// IAuthenticationStrategy.cs
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public interface IAuthenticationStrategy
    {
        Task ApplyAsync(ExecutionContext context, object component);
    }
}