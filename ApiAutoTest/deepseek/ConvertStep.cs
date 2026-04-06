// ConvertStep.cs (full implementation)
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TestAutomationEngine.Core
{
    public class ConvertStep : ComponentBase
    {
        public enum ConvertOperation
        {
            Base64Encode, Base64Decode,
            HexEncode, HexDecode,
            UrlEncode, UrlDecode,
            JsonEscape, JsonUnescape,
            ToUpper, ToLower
        }

        public ConvertOperation Operation { get; set; }
        public string Input { get; set; } = string.Empty;
        public string TargetVariable { get; set; } = string.Empty;

        public override string ComponentType => "Convert";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var inputValue = VariableResolver.Resolve(Input, context);
            string result = Operation switch
            {
                ConvertOperation.Base64Encode => Convert.ToBase64String(Encoding.UTF8.GetBytes(inputValue)),
                ConvertOperation.Base64Decode => Encoding.UTF8.GetString(Convert.FromBase64String(inputValue)),
                ConvertOperation.HexEncode => BitConverter.ToString(Encoding.UTF8.GetBytes(inputValue)).Replace("-", ""),
                ConvertOperation.HexDecode => Encoding.UTF8.GetString(HexStringToBytes(inputValue)),
                ConvertOperation.UrlEncode => Uri.EscapeDataString(inputValue),
                ConvertOperation.UrlDecode => Uri.UnescapeDataString(inputValue),
                ConvertOperation.JsonEscape => System.Text.Json.JsonSerializer.Serialize(inputValue).Trim('"'),
                ConvertOperation.JsonUnescape => System.Text.Json.JsonSerializer.Deserialize<string>("\"" + inputValue + "\"") ?? inputValue,
                ConvertOperation.ToUpper => inputValue.ToUpper(),
                ConvertOperation.ToLower => inputValue.ToLower(),
                _ => inputValue
            };
            context.SetVariable(TargetVariable, result);
            return new ComponentResult { Component = this, Output = result };
        }

        private static byte[] HexStringToBytes(string hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}