namespace Bloom.Events;

public delegate Task TrackStuckEvent(TrackStuckEventArgs args);

public sealed class TrackStuckEventArgs : TrackEventArgs
{
    public TimeSpan Threshold { get; }

    internal TrackStuckEventArgs(BloomPlayer player, TimeSpan threshold) : base(player)
    {
        Threshold = threshold;
    }
}
