// TestPlan.cs
namespace TestAutomationEngine.Core
{
    public class TestPlan : ContainerComponentBase
    {
        public override string ComponentType => "TestPlan";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            return await ExecuteChildrenAsync(context);
        }
    }
}