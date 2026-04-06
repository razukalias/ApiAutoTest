// ParallelPlanRunner.cs
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class ParallelPlanRunner
    {
        private readonly Project _project;
        public int MaxConcurrency { get; set; } = Environment.ProcessorCount;

        public ParallelPlanRunner(Project project)
        {
            _project = project;
        }

        public async Task<List<ComponentResult>> RunAllPlansAsync()
        {
            var results = new ConcurrentBag<ComponentResult>();
            await Parallel.ForEachAsync(_project.Children, new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrency }, async (plan, ct) =>
            {
                var context = new ExecutionContext(cancellationToken: ct);
                var result = await plan.ExecuteAsync(context);
                results.Add(result);
            });
            return results.ToList();
        }
    }
}