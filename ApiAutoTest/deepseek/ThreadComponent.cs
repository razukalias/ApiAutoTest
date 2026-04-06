// ThreadComponent.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class ThreadComponent : ContainerComponentBase
    {
        public int ThreadCount { get; set; } = 1;

        public override string ComponentType => "Thread";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var tasks = new List<Task<ComponentResult>>();
            for (int i = 0; i < ThreadCount; i++)
            {
                var isolatedContext = context.CloneForThread();
                tasks.Add(Task.Run(() => ExecuteChildrenAsync(isolatedContext)));
            }

            var childResults = await Task.WhenAll(tasks);
            var aggregated = new ComponentResult { Component = this };
            foreach (var r in childResults)
            {
                foreach (var kv in r.VariableChanges)
                    aggregated.VariableChanges[kv.Key] = kv.Value;
                aggregated.AssertionResults.AddRange(r.AssertionResults);
            }
            return aggregated;
        }
    }
}