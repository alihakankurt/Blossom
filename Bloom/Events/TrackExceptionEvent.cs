namespace Bloom.Events;

public delegate Task TrackExceptionEvent(TrackExceptionEventArgs args);

public sealed class TrackExceptionEventArgs : TrackEventArgs
{
    public TrackEventException Exception { get; }

    internal TrackExceptionEventArgs(BloomPlayer player, TrackEventException exception) : base(player)
    {
        Exception = exception;
    }
}

public sealed class TrackEventException
{
    public string Message { get; }
    public string Cause { get; }
    public TrackEventExceptionSeverity Severity { get; }

    internal TrackEventException(string message, string cause, TrackEventExceptionSeverity severity)
    {
        Message = message;
        Cause = cause;
        Severity = severity;
    }
}
