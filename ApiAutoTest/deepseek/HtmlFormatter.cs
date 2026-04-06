using System.Text;
using TestAutomationEngine.Core;

namespace TestAutomationEngine.Reporting
{
    public class HtmlFormatter : IReportFormatter
    {
        public string Format(ProjectResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><title>Test Report</title></head><body>");
            sb.AppendLine($"<h1>{result.ProjectName}</h1>");
            sb.AppendLine($"<p>Run at: {result.RunTimestamp}</p>");
            sb.AppendLine($"<p>Environment: {result.EnvironmentUsed}</p>");
            sb.AppendLine($"<p>Overall: {(result.Success ? "PASSED" : "FAILED")}</p>");
            sb.AppendLine("<table border='1'><tr><th>TestPlan</th><th>Duration (ms)</th><th>Status</th><th>Details</th></tr>");
            foreach (var planResult in result.TestPlanResults)
            {
                sb.AppendLine($"<tr style='background:{(planResult.Success ? "#a0ffa0" : "#ffa0a0")}'>");
                sb.AppendLine($"<td>{planResult.Component.Name}</td>");
                sb.AppendLine($"<td>{planResult.DurationMs}</td>");
                sb.AppendLine($"<td>{(planResult.Success ? "Passed" : "Failed")}</td>");
                sb.AppendLine($"<td>{planResult.Exception?.Message ?? ""}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }
    }
}