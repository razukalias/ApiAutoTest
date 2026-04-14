using Microsoft.Extensions.Logging;
using System.Text;
using System.Windows.Controls;

namespace TestAutomation.UI.Wpf;

public class TextBoxWriter : System.IO.TextWriter
{
    private readonly TextBox _textBox;
    public TextBoxWriter(TextBox textBox) => _textBox = textBox;

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        _textBox.Dispatcher.Invoke(() => _textBox.AppendText(value.ToString()));
    }

    public override void Write(string? value)
    {
        _textBox.Dispatcher.Invoke(() => _textBox.AppendText(value));
    }

    public override void WriteLine(string? value)
    {
        _textBox.Dispatcher.Invoke(() => _textBox.AppendText(value + "\n"));
    }
}

public class TextBoxLoggerProvider : ILoggerProvider
{
    private readonly TextBoxWriter _writer;
    public TextBoxLoggerProvider(TextBoxWriter writer) => _writer = writer;

    public ILogger CreateLogger(string categoryName) => new TextBoxLogger(_writer);
    public void Dispose() { }
}

public class TextBoxLogger : ILogger
{
    private readonly TextBoxWriter _writer;
    public TextBoxLogger(TextBoxWriter writer) => _writer = writer;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _writer.WriteLine($"[{logLevel}] {message}");
    }
}