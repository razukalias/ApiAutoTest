using System.Text;
using System.Xml.Linq;
using TestAutomationEngine.Core;

namespace TestAutomationEngine.Reporting
{
    public class JUnitFormatter : IReportFormatter
    {
        public string Format(ProjectResult result)
        {
            var ts = new XElement("testsuites");
            var suite = new XElement("testsuite",
                new XAttribute("name", result.ProjectName),
                new XAttribute("timestamp", result.RunTimestamp.ToString("o")),
                new XAttribute("tests", result.TestPlanResults.Count),
                new XAttribute("failures", result.TestPlanResults.Count(r => !r.Success)),
                new XAttribute("time", result.TestPlanResults.Sum(r => r.DurationMs) / 1000.0)
            );
            foreach (var planResult in result.TestPlanResults)
            {
                var testcase = new XElement("testcase",
                    new XAttribute("name", planResult.Component.Name),
                    new XAttribute("classname", result.ProjectName),
                    new XAttribute("time", planResult.DurationMs / 1000.0)
                );
                if (!planResult.Success)
                {
                    var failure = new XElement("failure",
                        new XAttribute("message", planResult.Exception?.Message ?? "Assertion failed"),
                        planResult.Exception?.ToString() ?? string.Join(", ", planResult.AssertionResults.Where(a => !a.Passed).Select(a => a.Message))
                    );
                    testcase.Add(failure);
                }
                suite.Add(testcase);
            }
            ts.Add(suite);
            return ts.ToString();
        }
    }
}