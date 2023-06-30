namespace Bloom.Events;

public delegate Task NodeExceptionEvent(NodeExceptionEventArgs args);

public sealed class NodeExceptionEventArgs : IEventArgs
{
    public string Message { get; }

    internal NodeExceptionEventArgs(string message)
    {
        Message = message;
    }
}
