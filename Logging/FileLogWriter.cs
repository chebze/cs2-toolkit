using Cs2Toolkit.Configuration;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Logging;

public sealed class FileLogWriter : IDisposable
{
    private readonly object _lock = new();
    private readonly StreamWriter? _writer;

    public bool IsEnabled { get; }
    public string FilePath { get; }

    public FileLogWriter(IOptions<ToolkitOptions> options)
    {
        var settings = options.Value.FileLogging;
        IsEnabled = settings.Enabled;

        if (!IsEnabled)
        {
            FilePath = string.Empty;
            return;
        }

        var directory = Path.GetFullPath(settings.Directory);
        Directory.CreateDirectory(directory);

        var fileName = $"{settings.FileNamePrefix}-{DateTime.Now:yyyy-MM-dd-HHmmss}.log";
        FilePath = Path.Combine(directory, fileName);

        _writer = new StreamWriter(FilePath, append: true) { AutoFlush = true };
        Write("SESSION", $"Log file created at {FilePath}");
    }

    public void Write(string category, string message)
    {
        if (!IsEnabled || _writer is null)
            return;

        lock (_lock)
        {
            _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{category}] {message}");
        }
    }

    public void Dispose()
    {
        if (!IsEnabled || _writer is null)
            return;

        lock (_lock)
        {
            Write("SESSION", "Log file closed");
            _writer.Dispose();
        }
    }
}
