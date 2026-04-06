// OAuth2ClientCredentials.cs
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class OAuth2ClientCredentials : IAuthenticationStrategy
    {
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public bool AutoRefresh { get; set; } = true;
        public string TokenVariableName { get; set; } = "token";

        public async Task ApplyAsync(ExecutionContext context, object component)
        {
            var resolvedEndpoint = VariableResolver.Resolve(TokenEndpoint, context);
            using var client = new HttpClient();
            var body = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["scope"] = string.Join(" ", Scopes)
            };
            var response = await client.PostAsync(resolvedEndpoint, new FormUrlEncodedContent(body));
            var json = await response.Content.ReadAsStringAsync();
            var tokenObj = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            var token = tokenObj?["access_token"]?.ToString();
            context.SetVariable(TokenVariableName, token, isMutable: false);

            if (component is HttpStep http)
                http.Headers["Authorization"] = $"Bearer {token}";
            else if (component is GraphQLStep gql)
                gql.Headers["Authorization"] = $"Bearer {token}";
        }
    }
}