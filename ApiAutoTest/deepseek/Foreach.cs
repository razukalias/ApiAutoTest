// Foreach.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestAutomationEngine.Core
{
    public class Foreach : ContainerComponentBase
    {
        public object SourceDataPayload { get; set; } = new object();
        public string ItemVariableName { get; set; } = "item";

        public override string ComponentType => "Foreach";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var items = ResolveSourceItems(SourceDataPayload, context);
            var result = new ComponentResult { Component = this };
            foreach (var item in items)
            {
                context.SetVariable(ItemVariableName, item);
                var iterResult = await ExecuteChildrenAsync(context);
                foreach (var kv in iterResult.VariableChanges)
                    result.VariableChanges[kv.Key] = kv.Value;
                result.AssertionResults.AddRange(iterResult.AssertionResults);
            }
            return result;
        }

        private IEnumerable<object?> ResolveSourceItems(object source, ExecutionContext context)
        {
            // Handle common types: IEnumerable, DataTable, range string "1..10"
            if (source is IEnumerable enumerable && source is not string)
            {
                foreach (var item in enumerable)
                    yield return item;
            }
            else if (source is string rangeStr && rangeStr.Contains(".."))
            {
                var parts = rangeStr.Split("..");
                int start = int.Parse(parts[0]);
                int end = int.Parse(parts[1]);
                for (int i = start; i <= end; i++)
                    yield return i;
            }
            else
            {
                yield return source;
            }
        }
    }
}