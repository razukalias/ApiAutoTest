// IReportFormatter.cs
using TestAutomationEngine.Core;

namespace TestAutomationEngine.Reporting
{
    public interface IReportFormatter
    {
        string Format(ProjectResult result);
    }
}