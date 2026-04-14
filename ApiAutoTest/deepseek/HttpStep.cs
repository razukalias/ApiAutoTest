// HttpStep.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public class HttpStep : ComponentBase
    {
        public string Url { get; set; } = string.Empty;
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> Cookies { get; set; } = new();
        public object? Body { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public IAuthenticationStrategy? Authentication { get; set; }
        public List<ExtractionRule> Extractions { get; set; } = new();

        public override string ComponentType => "Http";
        [System.Text.Json.Serialization.JsonIgnore]
        public string BodyString
        {
            get => Body?.ToString() ?? string.Empty;
            set => Body = value;
        }
        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var resolvedUrl = VariableResolver.Resolve(Url, context);
            var request = new HttpRequestMessage(Method, resolvedUrl);
            foreach (var h in Headers)
                request.Headers.TryAddWithoutValidation(h.Key, VariableResolver.Resolve(h.Value, context));

            if (Body != null)
                request.Content = new StringContent(VariableResolver.Resolve(Body.ToString(), context), System.Text.Encoding.UTF8, "application/json");

            if (Authentication != null)
                await Authentication.ApplyAsync(context, this);

            using var httpClient = new HttpClient();
            httpClient.Timeout = Timeout;
            var response = await httpClient.SendAsync(request, context.CancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync();

            foreach (var extract in Extractions)
            {
                var value = extract.Extract(new { StatusCode = response.StatusCode, Body = responseBody, Headers = response.Headers });
                context.SetVariable(extract.TargetVariable, value);
            }

            return new ComponentResult { Component = this, Output = responseBody };
        }
    }
}