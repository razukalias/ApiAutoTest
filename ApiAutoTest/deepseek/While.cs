// While.cs


namespace TestAutomationEngine.Core
{
    public class While : ContainerComponentBase
    {
        public string Condition { get; set; } = string.Empty;

        public override string ComponentType => "While";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var evaluator = new ConditionEvaluator();
            var result = new ComponentResult { Component = this };
            while (evaluator.Evaluate(Condition, context))
            {
                var iterResult = await ExecuteChildrenAsync(context);
                foreach (var kv in iterResult.VariableChanges)
                    result.VariableChanges[kv.Key] = kv.Value;
                result.AssertionResults.AddRange(iterResult.AssertionResults);
            }
            return result;
        }
    }
}