using System;
using System.Collections.Generic;

namespace TestAutomationEngine.Core
{
    public class TestExecutionRequest
    {

        /// <summary>Project JSON string (or you can pass a Project object).</summary>
        public Project Project { get; set; }
        //public string ProjectJson { get; set; }

        /// <summary>Environment name (e.g., "Staging", "Production").</summary>
        public string EnvironmentName { get; set; }

        /// <summary>Global log level for this run.</summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Summary;

        /// <summary>If true, run all test plans. Otherwise run only those specified in TestPlanNames.</summary>
        public bool RunAllTestPlans { get; set; } = true;

        /// <summary>Names of test plans to run (only used if RunAllTestPlans = false).</summary>
        public List<string> TestPlanNames { get; set; } = new();

        /// <summary>If provided, run only components whose name or GUID matches these filters.</summary>
        public List<ComponentFilter> ComponentFilters { get; set; } = new();

        /// <summary>If true, stop execution after first failure (fatal assertion).</summary>
        public bool StopOnFirstFailure { get; set; } = false;

        /// <summary>Include execution history in result (for UI).</summary>
        public bool IncludeHistory { get; set; } = false;
    }

    public class ComponentFilter
    {
        public string Name { get; set; }      // exact name or regex pattern
        public string Guid { get; set; }      // GUID as string
        public bool UseRegex { get; set; } = false;
    }
}