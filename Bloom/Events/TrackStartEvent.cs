using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Represents an asynchronous event handler that is called when a track starts.
/// </summary>
/// <param name="args">The <see cref="TrackStartEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task TrackStartEvent(TrackStartEventArgs args);

/// <summary>
/// Provides data for the <see cref="TrackStartEvent"/>.
/// </summary>
public readonly struct TrackStartEventArgs : ITrackEventArgs
{
    /// <inheritdoc/>
    public readonly BloomPlayer Player { get; }

    internal TrackStartEventArgs(BloomPlayer player)
    {
        Player = player;
    }
}
