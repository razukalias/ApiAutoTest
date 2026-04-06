// WindowsIntegratedStrategy.cs
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class WindowsIntegratedStrategy : IAuthenticationStrategy
    {
        public async Task ApplyAsync(ExecutionContext context, object component)
        {
            // For SqlStep, set UseWindowsAuth = true; for Http, this would use HttpClientHandler.UseDefaultCredentials
            if (component is SqlStep sql)
                sql.UseWindowsAuth = true;
            // For HttpStep, you'd need to pass a HttpClientHandler with UseDefaultCredentials = true
        }
    }
}