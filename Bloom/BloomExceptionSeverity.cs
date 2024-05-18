namespace Bloom;

/// <summary>
/// Represents the severity of a <see cref="BloomException"/>.
/// </summary>
public enum BloomExceptionSeverity
{
    /// <summary>
    /// Indicates a common exception.
    /// </summary>
    Common,

    /// <summary>
    /// Indicates a unexpected exception.
    /// </summary>
    Suspicious,

    /// <summary>
    /// Indicates a critical exception.
    /// </summary>
    Fault,
}
