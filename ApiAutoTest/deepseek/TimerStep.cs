// TimerStep.cs
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class TimerStep : ContainerComponentBase
    {
        public override string ComponentType => "Timer";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            ComponentResult? childResult = null;
            if (Children.Count > 0)
                childResult = await ExecuteChildrenAsync(context);
            stopwatch.Stop();

            // Create a new dictionary and copy the existing VariableChanges to it
            var variableChanges = new Dictionary<string, object?>(childResult?.VariableChanges ?? new Dictionary<string, object?>());

            return new ComponentResult
            {
                Component = this,
                Output = new { ElapsedMs = stopwatch.ElapsedMilliseconds },
                // Use a method to populate VariableChanges since it is read-only
                VariableChanges = variableChanges.ToDictionary(entry => entry.Key, entry => entry.Value)
            };
        }
    }
}