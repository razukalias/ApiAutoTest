// ConditionEvaluator.cs (complete)
using DynamicExpresso;
using System.Linq;

namespace TestAutomationEngine.Core
{
    public class ConditionEvaluator
    {
        private readonly Interpreter _interpreter = new();

        public bool Evaluate(string expression, ExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return true;

            var allVars = context.GetAllVariables();
            var parameters = allVars.Select(kvp => new Parameter(kvp.Key, kvp.Value?.GetType() ?? typeof(object), kvp.Value));
            return _interpreter.Eval<bool>(expression, parameters.ToArray());
        }
    }
}