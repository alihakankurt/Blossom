namespace Bloom.Events;

/// <summary>
/// Provides data for the <see cref="NodeExceptionEvent"/> event.
/// </summary>
public sealed class NodeExceptionEventArgs
{
    /// <summary>
    /// Gets the message of the exception.
    /// </summary>
    public string Message { get; internal init; }

    internal NodeExceptionEventArgs(string message)
    {
        Message = message;
    }
}
