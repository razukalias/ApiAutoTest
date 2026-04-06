// ITestComponent.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public interface ITestComponent
    {
        string Name { get; }
        Guid Guid { get; }
        IReadOnlyDictionary<string, Variable> ScopedVariables { get; }
        IReadOnlyList<IAssertion> Assertions { get; }
        ScriptStep? BeforeExecute { get; }
        ScriptStep? AfterExecute { get; }
        RetryPolicy? RetryPolicy { get; }
        string ComponentType { get; }
        Task<ComponentResult> ExecuteAsync(ExecutionContext context);
    }

    public interface IContainerComponent : ITestComponent
    {
        IReadOnlyList<ITestComponent> Children { get; }
    }
}