// ComponentBase.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public abstract class ComponentBase : ITestComponent
    {
        public string Name { get; set; } = string.Empty;
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Dictionary<string, Variable> ScopedVariables { get; } = new();
        public List<IAssertion> Assertions { get; } = new();
        public ScriptStep? BeforeExecute { get; set; }
        public ScriptStep? AfterExecute { get; set; }
        public RetryPolicy? RetryPolicy { get; set; }
        public abstract string ComponentType { get; }

        IReadOnlyDictionary<string, Variable> ITestComponent.ScopedVariables => ScopedVariables;
        IReadOnlyList<IAssertion> ITestComponent.Assertions => Assertions;

        public async Task<ComponentResult> ExecuteAsync(ExecutionContext context)
        {
            using var _ = context.EnterScope(ScopedVariables);

            ComponentResult result = null;
            Exception? failure = null;

            try
            {
                if (BeforeExecute != null)
                    await BeforeExecute.ExecuteAsync(context);

                result = await ExecuteWithRetryAsync(context);

                foreach (var assertion in Assertions)
                {
                    var assertionResult = await assertion.AssertAsync(result, context);
                    result.AssertionResults.Add(assertionResult);
                    if (assertionResult.Behavior == AssertionBehavior.Fatal && !assertionResult.Passed)
                        throw new AssertionFailedException(assertionResult.Message);
                }
            }
            catch (Exception ex)
            {
                failure = ex;
                if (result == null)
                {
                    result = new ComponentResult { Component = this, HasUnhandledException = true, Exception = ex };
                }
                else
                {
                    result.HasUnhandledException = true;
                    result.Exception = ex;
                }
            }
            finally
            {
                if (AfterExecute != null)
                    await AfterExecute.ExecuteAsync(context);
            }

            if (failure != null && failure is AssertionFailedException)
                throw failure;

            return result;
        }

        private async Task<ComponentResult> ExecuteWithRetryAsync(ExecutionContext context)
        {
            var policy = RetryPolicy ?? new RetryPolicy { MaxAttempts = 1 };
            int attempt = 0;
            Exception? lastEx = null;

            while (attempt < policy.MaxAttempts)
            {
                try
                {
                    var result = await ExecuteCoreAsync(context);
                    result.RetryCount = attempt;
                    return result;
                }
                catch (Exception ex) when (policy.ShouldRetry(ex))
                {
                    lastEx = ex;
                    attempt++;
                    if (attempt < policy.MaxAttempts)
                        await Task.Delay(policy.GetDelay(attempt), context.CancellationToken);
                }
            }

            throw new MaxRetryExceededException($"Failed after {policy.MaxAttempts} attempts", lastEx);
        }

        protected abstract Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context);
    }

    public class AssertionFailedException : Exception
    {
        public AssertionFailedException(string message) : base(message) { }
    }

    public class MaxRetryExceededException : Exception
    {
        public MaxRetryExceededException(string message, Exception? inner) : base(message, inner) { }
    }
}