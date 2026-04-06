// JsonPathEvaluator.cs
using Newtonsoft.Json.Linq;
using System;

namespace TestAutomationEngine.Core
{
    public static class JsonPathEvaluator
    {
        public static object? Evaluate(object? data, string jsonPath)
        {
            if (data == null || string.IsNullOrEmpty(jsonPath))
                return null;

            try
            {
                JToken token;
                if (data is string jsonString)
                    token = JToken.Parse(jsonString);
                else if (data is JToken jt)
                    token = jt;
                else
                    token = JToken.FromObject(data);

                var result = token.SelectToken(jsonPath);
                return result?.ToObject<object>();
            }
            catch
            {
                return null;
            }
        }
    }
}