using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Provides data for the <see cref="TrackEndEvent"/> event.
/// </summary>
public sealed class TrackEndEventArgs : TrackEventArgs
{
    /// <summary>
    /// Gets the reason why the track ended.
    /// </summary>
    public TrackEndReason EndReason { get; }

    /// <summary>
    /// Gets a value indicating whether the next track may start.
    /// </summary>
    public bool MayStartNext => EndReason is TrackEndReason.Finished or TrackEndReason.LoadFailed;

    internal TrackEndEventArgs(BloomPlayer player, TrackEndReason endReason) : base(player)
    {
        EndReason = endReason;
    }
}
