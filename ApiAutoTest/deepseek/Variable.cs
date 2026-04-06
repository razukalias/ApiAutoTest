// Variable.cs
using System;

namespace TestAutomationEngine.Core
{
    public class Variable
    {
        public object? Value { get; set; }
        public bool IsMutable { get; set; }
        public bool IsSensitive { get; set; }

        public Variable(object? value, bool isMutable = true, bool isSensitive = false)
        {
            Value = value;
            IsMutable = isMutable;
            IsSensitive = isSensitive;
        }

        public Variable Clone() => new Variable(Value, IsMutable, IsSensitive);
    }
}