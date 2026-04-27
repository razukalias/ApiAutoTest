using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace TestAutomation.UI.Wpf;

public class ObservableLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Action<string> _logAction;

    public ObservableLogger(string categoryName, Action<string> logAction)
    {
        _categoryName = categoryName;
        _logAction = logAction;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logAction($"[{logLevel}] {message}");
    }
}

public class ObservableLoggerProvider : ILoggerProvider
{
    private readonly Action<string> _logAction;

    public ObservableLoggerProvider(Action<string> logAction) => _logAction = logAction;

    public ILogger CreateLogger(string categoryName) => new ObservableLogger(categoryName, _logAction);
    public void Dispose() { }
}