// ComponentResult.cs
using System;
using System.Collections.Generic;


namespace TestAutomationEngine.Core
{
    public class ComponentResult
    {
        public ITestComponent Component { get; set; }
        public object? Output { get; set; }
        public List<AssertionResult> AssertionResults { get; } = new();
        public Dictionary<string, object?> VariableChanges { get; set; } = new();
        public int RetryCount { get; set; }
        public long DurationMs { get; set; }
        public bool Success => !HasUnhandledException && AssertionResults.TrueForAll(a => a.Passed);
        public bool HasUnhandledException { get; set; }
        public Exception? Exception { get; set; }
    }
}