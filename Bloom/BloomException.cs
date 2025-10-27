namespace Bloom;

/// <summary>
/// Represents the severity of a <see cref="BloomException"/>.
/// </summary>
public sealed class BloomException
{
    /// <summary>
    /// Gets the message of the exception.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the severity of the exception.
    /// </summary>
    public BloomExceptionSeverity Severity { get; }

    /// <summary>
    /// Gets the cause of the exception.
    /// </summary>
    public string Cause { get; }

    internal BloomException(string? message, BloomExceptionSeverity severity, string cause)
    {
        Message = message;
        Severity = severity;
        Cause = cause;
    }
}
