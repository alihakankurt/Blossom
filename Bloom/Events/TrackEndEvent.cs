namespace Bloom.Events;

public delegate Task TrackEndEvent(TrackEndEventArgs args);

public sealed class TrackEndEventArgs : TrackEventArgs
{
    public TrackEndReason EndReason { get; }

    public bool ShouldPlayNext => EndReason is TrackEndReason.Finished or TrackEndReason.LoadFailed;

    internal TrackEndEventArgs(BloomPlayer player, TrackEndReason endReason) : base(player)
    {
        EndReason = endReason;
    }
}
