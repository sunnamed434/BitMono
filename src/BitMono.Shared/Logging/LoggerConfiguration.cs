namespace BitMono.Shared.Logging;

/// <summary>
/// Configuration options for the logger.
/// </summary>
public class LoggerConfiguration
{
    /// <summary>
    /// Minimum log level to output. Messages below this level are ignored.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Whether to write log messages to the console.
    /// </summary>
    public bool WriteToConsole { get; set; } = true;

    /// <summary>
    /// Whether to write log messages to a file.
    /// </summary>
    public bool WriteToFile { get; set; }

    /// <summary>
    /// Path to the log file. Required if WriteToFile is true.
    /// </summary>
    public string? LogFilePath { get; set; }
}
