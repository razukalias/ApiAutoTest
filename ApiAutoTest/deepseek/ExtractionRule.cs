// ExtractionRule.cs (complete)
using System.Text.RegularExpressions;

namespace TestAutomationEngine.Core
{
    public class ExtractionRule
    {
        public string Source { get; set; } = string.Empty;
        public string TargetVariable { get; set; } = string.Empty;
        public ExtractionType Type { get; set; } = ExtractionType.JsonPath;

        public object? Extract(object data)
        {
            return Type switch
            {
                ExtractionType.JsonPath => JsonPathEvaluator.Evaluate(data, Source),
                ExtractionType.XPath => XPathEvaluator.Evaluate(data, Source),
                ExtractionType.Regex => Regex.Match(data?.ToString() ?? "", Source).Value,
                _ => null
            };
        }
    }

    public enum ExtractionType { JsonPath, XPath, Regex }
}