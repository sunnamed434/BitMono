using System.Text.RegularExpressions;

namespace BitMono.Shared.Logging;

/// <summary>
/// Lightweight logger implementation with console and file output.
/// </summary>
public class Logger : ILogger
{
    private readonly string? _context;
    private readonly LoggerConfiguration _configuration;
    private static readonly object ConsoleLock = new();
    private static readonly object FileLock = new();

    /// <summary>
    /// Creates a new logger with the specified configuration.
    /// </summary>
    /// <param name="configuration">Logger configuration</param>
    /// <param name="context">Optional source context name</param>
    public Logger(LoggerConfiguration configuration, string? context = null)
    {
        _configuration = configuration;
        _context = context;
    }

    /// <summary>
    /// Creates a new logger with default configuration (console output only).
    /// </summary>
    public Logger() : this(new LoggerConfiguration())
    {
    }

    /// <inheritdoc/>
    public ILogger ForContext<T>() => ForContext(typeof(T));

    /// <inheritdoc/>
    public ILogger ForContext(Type type) => new Logger(_configuration, type.Name);

    /// <inheritdoc/>
    public void Debug(string messageTemplate, params object[] args)
        => Log(LogLevel.Debug, null, messageTemplate, args);

    /// <inheritdoc/>
    public void Information(string messageTemplate, params object[] args)
        => Log(LogLevel.Information, null, messageTemplate, args);

    /// <inheritdoc/>
    public void Warning(string messageTemplate, params object[] args)
        => Log(LogLevel.Warning, null, messageTemplate, args);

    /// <inheritdoc/>
    public void Error(string messageTemplate, params object[] args)
        => Log(LogLevel.Error, null, messageTemplate, args);

    /// <inheritdoc/>
    public void Error(Exception exception, string messageTemplate, params object[] args)
        => Log(LogLevel.Error, exception, messageTemplate, args);

    /// <inheritdoc/>
    public void Fatal(string messageTemplate, params object[] args)
        => Log(LogLevel.Fatal, null, messageTemplate, args);

    /// <inheritdoc/>
    public void Fatal(Exception exception, string messageTemplate, params object[] args)
        => Log(LogLevel.Fatal, exception, messageTemplate, args);

    private void Log(LogLevel level, Exception? exception, string messageTemplate, object[] args)
    {
        if (level < _configuration.MinimumLevel)
            return;

        var message = FormatMessage(messageTemplate, args);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var levelStr = GetLevelString(level);
        var contextStr = _context ?? "Program";

        var logLine = $"[{timestamp} {levelStr}][{contextStr}] {message}";

        if (exception != null)
            logLine += Environment.NewLine + exception;

        if (_configuration.WriteToConsole)
            WriteToConsole(level, logLine);

        if (_configuration.WriteToFile && !string.IsNullOrEmpty(_configuration.LogFilePath))
            WriteToFile(logLine);
    }

    private static string FormatMessage(string template, object[] args)
    {
        if (args.Length == 0)
            return template;

        var result = template;

        // Support {0}, {1}, etc. style placeholders
        for (int i = 0; i < args.Length; i++)
        {
            var pattern = $"{{{i}}}";
            result = result.Replace(pattern, args[i]?.ToString() ?? "null");
        }

        // Support {Name} style placeholders (Serilog-style) - replace with positional args in order
        var namedPattern = new Regex(@"\{([^{}0-9]+)\}");
        var matches = namedPattern.Matches(result);
        int argIndex = 0;
        foreach (Match match in matches)
        {
            if (argIndex < args.Length)
            {
                result = result.Replace(match.Value, args[argIndex]?.ToString() ?? "null");
                argIndex++;
            }
        }

        return result;
    }

    private static string GetLevelString(LogLevel level) => level switch
    {
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Fatal => "FTL",
        _ => "???"
    };

    private static void WriteToConsole(LogLevel level, string message)
    {
        lock (ConsoleLock)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.Gray
            };
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
    }

    private void WriteToFile(string message)
    {
        try
        {
            lock (FileLock)
            {
                var dir = Path.GetDirectoryName(_configuration.LogFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.AppendAllText(_configuration.LogFilePath!, message + Environment.NewLine);
            }
        }
        catch
        {
            // Silently fail file logging to avoid disrupting application
        }
    }
}
