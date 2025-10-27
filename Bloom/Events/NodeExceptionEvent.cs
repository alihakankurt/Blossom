namespace Bloom.Events;

/// <summary>
/// Represents an asynchronous event handler that is called when an exception thrown.
/// </summary>
/// <param name="args">The <see cref="NodeExceptionEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
public delegate ValueTask NodeExceptionEvent(NodeExceptionEventArgs args);

/// <summary>
/// Provides data for the <see cref="NodeExceptionEvent"/>.
/// </summary>
public readonly struct NodeExceptionEventArgs
{
    /// <summary>
    /// Gets the message of the exception.
    /// </summary>
    public readonly string Message { get; }

    internal NodeExceptionEventArgs(string message)
    {
        Message = message;
    }
}
