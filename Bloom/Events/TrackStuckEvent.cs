using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Represents an asynchronous event handler that is called when current track gets stuck.
/// </summary>
/// <param name="args">The <see cref="TrackStuckEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
public delegate ValueTask TrackStuckEvent(TrackStuckEventArgs args);


/// <summary>
/// Provides data for the <see cref="TrackStuckEvent"/>.
/// </summary>
public readonly struct TrackStuckEventArgs : ITrackEventArgs
{
    /// <inheritdoc/>
    public readonly BloomPlayer Player { get; }

    /// <summary>
    /// Gets the threshold that was exceeded.
    /// </summary>
    public readonly TimeSpan Threshold { get; }

    internal TrackStuckEventArgs(BloomPlayer player, TimeSpan threshold)
    {
        Player = player;
        Threshold = threshold;
    }
}
