using Cs2Toolkit.Logging;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLogWriter _fileLog;

    public FileLoggerProvider(FileLogWriter fileLog)
    {
        _fileLog = fileLog;
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _fileLog);

    public void Dispose()
    {
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly FileLogWriter _fileLog;

        public FileLogger(string category, FileLogWriter fileLog)
        {
            _category = category;
            _fileLog = fileLog;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => _fileLog.IsEnabled && logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (exception is not null)
                message = $"{message} | {exception}";

            _fileLog.Write(_category, message);
        }
    }
}
