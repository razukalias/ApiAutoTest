// Loop.cs
namespace TestAutomationEngine.Core
{
    public class Loop : ContainerComponentBase
    {
        public int Start { get; set; } = 1;
        public int End { get; set; } = 10;
        public int Step { get; set; } = 1;

        public override string ComponentType => "Loop";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var result = new ComponentResult { Component = this };
            for (int i = Start; i <= End; i += Step)
            {
                context.SetVariable("loopIndex", i);
                var iterResult = await ExecuteChildrenAsync(context);
                foreach (var kv in iterResult.VariableChanges)
                    result.VariableChanges[kv.Key] = kv.Value;
                result.AssertionResults.AddRange(iterResult.AssertionResults);
            }
            return result;
        }
    }
}