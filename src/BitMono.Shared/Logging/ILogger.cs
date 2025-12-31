namespace BitMono.Shared.Logging;

/// <summary>
/// Lightweight logging interface compatible with Serilog's common patterns.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Debug(string messageTemplate, params object[] args);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Information(string messageTemplate, params object[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Warning(string messageTemplate, params object[] args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Error(string messageTemplate, params object[] args);

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Error(Exception exception, string messageTemplate, params object[] args);

    /// <summary>
    /// Logs a fatal error message.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Fatal(string messageTemplate, params object[] args);

    /// <summary>
    /// Logs a fatal error message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="messageTemplate">Message template with optional placeholders</param>
    /// <param name="args">Arguments to substitute into placeholders</param>
    void Fatal(Exception exception, string messageTemplate, params object[] args);

    /// <summary>
    /// Creates a new logger with the specified source context.
    /// </summary>
    /// <typeparam name="T">Type to use as context</typeparam>
    /// <returns>Logger with source context</returns>
    ILogger ForContext<T>();

    /// <summary>
    /// Creates a new logger with the specified source context.
    /// </summary>
    /// <param name="type">Type to use as context</param>
    /// <returns>Logger with source context</returns>
    ILogger ForContext(Type type);
}
