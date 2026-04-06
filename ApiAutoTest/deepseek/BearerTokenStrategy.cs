// BearerTokenStrategy.cs
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class BearerTokenStrategy : IAuthenticationStrategy
    {
        public string Token { get; set; } = string.Empty;

        public async Task ApplyAsync(ExecutionContext context, object component)
        {
            var resolvedToken = VariableResolver.Resolve(Token, context);
            if (component is HttpStep http)
                http.Headers["Authorization"] = $"Bearer {resolvedToken}";
            else if (component is GraphQLStep gql)
                gql.Headers["Authorization"] = $"Bearer {resolvedToken}";
        }
    }
}