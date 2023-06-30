namespace Bloom.Events;

public delegate Task TrackStartEvent(TrackStartEventArgs args);

public sealed class TrackStartEventArgs : TrackEventArgs
{
    internal TrackStartEventArgs(BloomPlayer player) : base(player)
    {
    }
}
