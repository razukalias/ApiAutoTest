// ProjectResult.cs
using System;
using System.Collections.Generic;

namespace TestAutomationEngine.Core
{
    public class ProjectResult
    {
        public string ProjectName { get; set; } = string.Empty;
        public string EnvironmentUsed { get; set; } = string.Empty;
        public DateTime RunTimestamp { get; set; } = DateTime.UtcNow;
        public LogLevel LogLevel { get; set; }
        public List<ComponentResult> TestPlanResults { get; } = new();
        public bool Success => TestPlanResults.TrueForAll(r => r.Success);
    }
}