using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Represents an asynchronous event handler that is called when current track ends.
/// </summary>
/// <param name="args">The <see cref="TrackEndEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
public delegate ValueTask TrackEndEvent(TrackEndEventArgs args);

/// <summary>
/// Provides data for the <see cref="TrackEndEvent"/>.
/// </summary>
public readonly struct TrackEndEventArgs : ITrackEventArgs
{
    /// <inheritdoc/>
    public readonly BloomPlayer Player { get; }

    /// <summary>
    /// Gets the reason why the track ended.
    /// </summary>
    public readonly TrackEndReason EndReason { get; }

    /// <summary>
    /// Gets a value indicating whether the next track may start.
    /// </summary>
    public readonly bool MayStartNext => EndReason is TrackEndReason.Finished or TrackEndReason.LoadFailed;

    internal TrackEndEventArgs(BloomPlayer player, TrackEndReason endReason)
    {
        Player = player;
        EndReason = endReason;
    }
}
