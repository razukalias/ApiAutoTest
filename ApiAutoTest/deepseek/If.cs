// If.cs


namespace TestAutomationEngine.Core
{
    public class If : ContainerComponentBase
    {
        public string Condition { get; set; } = string.Empty;

        public override string ComponentType => "If";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var evaluator = new ConditionEvaluator();
            if (evaluator.Evaluate(Condition, context))
                return await ExecuteChildrenAsync(context);
            return new ComponentResult { Component = this };
        }
    }
}