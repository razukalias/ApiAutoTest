using System;
using System.Collections.Generic;

namespace TestAutomationEngine.Core
{
    public class TestExecutionResult
    {
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public List<TestPlanResultSummary> TestPlanResults { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string FullReportJUnit { get; set; }    // optional JUnit string
        public string FullReportHtml { get; set; }     // optional HTML
        public object RawExecutionData { get; set; }   // full execution tree (for UI)
        public bool IncludeHistory { get; set; }
    }

    public class TestPlanResultSummary
    {
        public string Name { get; set; }
        public bool Success { get; set; }
        public long DurationMs { get; set; }
        public int AssertionsPassed { get; set; }
        public int AssertionsFailed { get; set; }
        public string ErrorMessage { get; set; }
    }
}