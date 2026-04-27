using System.Collections.Generic;
using System.Net.Http;

namespace TestAutomationEngine.Core;

public static class HttpMethodHelper
{
    public static IEnumerable<HttpMethod> SupportedMethods { get; } = new[]
    {
        HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete,
        HttpMethod.Patch, HttpMethod.Head, HttpMethod.Options, HttpMethod.Trace
    };
}