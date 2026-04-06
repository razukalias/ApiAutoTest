// ApiKeyStrategy.cs
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class ApiKeyStrategy : IAuthenticationStrategy
    {
        public string ApiKey { get; set; } = string.Empty;
        public string HeaderName { get; set; } = "X-API-Key";

        public async Task ApplyAsync(ExecutionContext context, object component)
        {
            var resolvedKey = VariableResolver.Resolve(ApiKey, context);
            if (component is HttpStep http)
                http.Headers[HeaderName] = resolvedKey;
            else if (component is GraphQLStep gql)
                gql.Headers[HeaderName] = resolvedKey;
        }
    }
}