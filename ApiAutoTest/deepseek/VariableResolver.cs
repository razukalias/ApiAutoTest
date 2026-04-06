// VariableResolver.cs
using System.Text.RegularExpressions;

namespace TestAutomationEngine.Core
{
    public static class VariableResolver
    {
        private static readonly Regex PlaceholderRegex = new(@"\$\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled);

        public static string Resolve(string input, ExecutionContext context)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return PlaceholderRegex.Replace(input, match =>
            {
                string varName = match.Groups[1].Value;
                var value = context.GetVariable(varName);
                return value?.ToString() ?? string.Empty;
            });
        }
    }
}