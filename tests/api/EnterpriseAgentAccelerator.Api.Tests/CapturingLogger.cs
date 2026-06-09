using Microsoft.Extensions.Logging;

namespace EnterpriseAgentAccelerator.Api.Tests;

internal sealed class CapturingLoggerProvider(List<string> messages) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, messages);

    public void Dispose()
    {
    }
}

internal sealed class CapturingLogger(string category, List<string> messages) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var line = $"[{category}] {formatter(state, exception)}";
        if (exception is not null)
        {
            line += $" {exception.GetType().Name}: {exception.Message}";
        }

        messages.Add(line);
    }
}

internal sealed class CapturingLogger<T>(List<string> messages) : ILogger<T>
{
    private readonly CapturingLogger _inner = new(typeof(T).FullName ?? typeof(T).Name, messages);

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => _inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        _inner.Log(logLevel, eventId, state, exception, formatter);
}
