namespace BitMono.Shared.Logging;

/// <summary>
/// Specifies the severity level of a log message.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Debug-level messages for development troubleshooting.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Informational messages that track the general flow of the application.
    /// </summary>
    Information = 1,

    /// <summary>
    /// Warning messages for abnormal or unexpected events.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error messages for failures within the current operation.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Fatal messages for unrecoverable application errors.
    /// </summary>
    Fatal = 4
}
