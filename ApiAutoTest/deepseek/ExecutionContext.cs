// ExecutionContext.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestAutomationEngine.Core
{
    public class ExecutionContext
    {
        private readonly Stack<Dictionary<string, Variable>> _scopeStack = new();
        private readonly ILogger _logger;
        private readonly LogLevel _minLogLevel;
        private readonly IDebugger? _debugger;

        public CancellationToken CancellationToken { get; }
        public string? EnvironmentName { get; set; }

        public ExecutionContext(ILogger? logger = null, LogLevel minLogLevel = LogLevel.Summary, CancellationToken cancellationToken = default, IDebugger? debugger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _minLogLevel = minLogLevel;
            CancellationToken = cancellationToken;
            _debugger = debugger;
            _scopeStack.Push(new Dictionary<string, Variable>()); // global scope
        }

        public object? GetVariable(string name)
        {
            foreach (var scope in _scopeStack)
            {
                if (scope.TryGetValue(name, out var var))
                    return var.Value;
            }
            return null;
        }

        public void SetVariable(string name, object? value, bool isMutable = true, bool isSensitive = false)
        {
            var currentScope = _scopeStack.Peek();
            if (currentScope.TryGetValue(name, out var existing))
            {
                if (!existing.IsMutable)
                    throw new InvalidOperationException($"Variable '{name}' is immutable and cannot be changed.");
                existing.Value = value;
                existing.IsSensitive = isSensitive;
            }
            else
            {
                currentScope[name] = new Variable(value, isMutable, isSensitive);
            }
        }

        public IDisposable EnterScope(IReadOnlyDictionary<string, Variable> scopedVariables)
        {
            var newScope = new Dictionary<string, Variable>();
            foreach (var kv in scopedVariables)
                newScope[kv.Key] = kv.Value.Clone();
            _scopeStack.Push(newScope);
            return new ScopePopper(_scopeStack);
        }

        public ExecutionContext CloneForThread()
        {
            var clone = new ExecutionContext(_logger, _minLogLevel, CancellationToken, _debugger);
            // Deep copy all scopes
            var newStack = new Stack<Dictionary<string, Variable>>();
            foreach (var scope in _scopeStack.Reverse())
            {
                var clonedScope = scope.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
                newStack.Push(clonedScope);
            }
            clone._scopeStack.Clear();
            foreach (var scope in newStack)
                clone._scopeStack.Push(scope);
            return clone;
        }

        public void Log(LogLevel level, string message, object? data = null)
        {
            if (level < _minLogLevel) return;
            // Mask sensitive data
            var maskedMessage = MaskSensitiveData(message);
            _logger.Log(level.ToMicrosoftLogLevel(), maskedMessage);
        }
        public IReadOnlyDictionary<string, object?> GetAllVariables()
        {
            var result = new Dictionary<string, object?>();
            foreach (var scope in _scopeStack)
            {
                foreach (var kv in scope)
                {
                    if (!result.ContainsKey(kv.Key))
                        result[kv.Key] = kv.Value.Value;
                }
            }
            return result;
        }
        private string MaskSensitiveData(string message)
        {
            foreach (var scope in _scopeStack)
            {
                foreach (var var in scope.Values.Where(v => v.IsSensitive && v.Value != null))
                {
                    message = message.Replace(var.Value.ToString(), "***");
                }
            }
            return message;
        }
       
        private class ScopePopper : IDisposable
        {
            private readonly Stack<Dictionary<string, Variable>> _stack;
            public ScopePopper(Stack<Dictionary<string, Variable>> stack) => _stack = stack;
            public void Dispose() => _stack.Pop();
        }
    }

    public static class LogLevelExtensions
    {
        public static Microsoft.Extensions.Logging.LogLevel ToMicrosoftLogLevel(this LogLevel level) => level switch
        {
            LogLevel.Off => Microsoft.Extensions.Logging.LogLevel.None,
            LogLevel.Errors => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Summary => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.ComponentExecution => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
        
    }
    // Add this method to ExecutionContext.cs

    }