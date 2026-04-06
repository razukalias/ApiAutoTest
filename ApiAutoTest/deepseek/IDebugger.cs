// IDebugger.cs
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public interface IDebugger
    {
        Task PauseBeforeComponent(ITestComponent component, ExecutionContext context);
        Task PauseAfterComponent(ITestComponent component, ExecutionContext context);
        Task Resume();
    }

    public class ConsoleDebugger : IDebugger
    {
        public Task PauseBeforeComponent(ITestComponent component, ExecutionContext context)
        {
            Console.WriteLine($"About to execute: {component.Name} ({component.ComponentType})");
            Console.WriteLine("Press Enter to continue, or type 'inspect' to view variables...");
            var input = Console.ReadLine();
            if (input == "inspect")
            {
                // Inspect logic
            }
            return Task.CompletedTask;
        }

        public Task PauseAfterComponent(ITestComponent component, ExecutionContext context) => Task.CompletedTask;
        public Task Resume() => Task.CompletedTask;
    }
}