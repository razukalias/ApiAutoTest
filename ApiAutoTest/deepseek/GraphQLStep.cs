// GraphQLStep.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public class GraphQLStep : ComponentBase
    {
        public string Url { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string? Mutation { get; set; }
        public object? Variables { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public IAuthenticationStrategy? Authentication { get; set; }
        public List<ExtractionRule> Extractions { get; set; } = new();

        public override string ComponentType => "GraphQL";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var resolvedUrl = VariableResolver.Resolve(Url, context);
            var operation = !string.IsNullOrEmpty(Mutation) ? Mutation : Query;
            var payload = new { query = operation, variables = Variables };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, resolvedUrl) { Content = content };
            foreach (var h in Headers)
                request.Headers.TryAddWithoutValidation(h.Key, VariableResolver.Resolve(h.Value, context));

            if (Authentication != null)
                await Authentication.ApplyAsync(context, this);

            var response = await httpClient.SendAsync(request, context.CancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync();

            foreach (var extract in Extractions)
            {
                var value = extract.Extract(responseBody);
                context.SetVariable(extract.TargetVariable, value);
            }

            return new ComponentResult { Component = this, Output = responseBody };
        }
    }
}