// RetryPolicy.cs
using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TestAutomationEngine.Core
{
    public class RetryPolicy
    {
        public int MaxAttempts { get; set; } = 1;
        public int BackoffMs { get; set; } = 1000;
        public bool ExponentialBackoff { get; set; } = false;
        public List<string> RetryOn { get; set; } = new();

        public bool ShouldRetry(Exception ex, HttpResponseMessage? response = null)
        {
            foreach (var pattern in RetryOn)
            {
                if (pattern.StartsWith("StatusCode:"))
                {
                    if (response != null)
                    {
                        var range = pattern.Substring(11);
                        if (range.Contains("-"))
                        {
                            var parts = range.Split('-');
                            int start = int.Parse(parts[0]);
                            int end = int.Parse(parts[1]);
                            if ((int)response.StatusCode >= start && (int)response.StatusCode <= end)
                                return true;
                        }
                        else if (int.TryParse(range, out int code) && (int)response.StatusCode == code)
                            return true;
                    }
                }
                else if (ex.GetType().Name.Contains(pattern) || Regex.IsMatch(ex.Message, pattern))
                {
                    return true;
                }
            }
            return false;
        }

        public int GetDelay(int attempt)
        {
            if (!ExponentialBackoff) return BackoffMs;
            return BackoffMs * (int)Math.Pow(2, attempt - 1);
        }
    }
}