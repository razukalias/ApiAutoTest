// ContainerComponentBase.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public abstract class ContainerComponentBase : ComponentBase, IContainerComponent
    {
        public List<ITestComponent> Children { get; set; } = new();
        IReadOnlyList<ITestComponent> IContainerComponent.Children => Children;

        protected async Task<ComponentResult> ExecuteChildrenAsync(ExecutionContext context)
        {
            var result = new ComponentResult { Component = this };
            foreach (var child in Children)
            {
                var childResult = await child.ExecuteAsync(context);
                foreach (var kv in childResult.VariableChanges)
                    result.VariableChanges[kv.Key] = kv.Value;
                result.AssertionResults.AddRange(childResult.AssertionResults);
                if (childResult.HasUnhandledException && childResult.Exception != null)
                    throw childResult.Exception;
            }
            return result;
        }
    }
}