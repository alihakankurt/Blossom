using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Provides data for an event that is related to a track.
/// </summary>
public abstract class TrackEventArgs
{
    /// <summary>
    /// Gets the <see cref="BloomPlayer"/> that the event is related to.
    /// </summary>
    public BloomPlayer Player { get; }

    internal TrackEventArgs(BloomPlayer player)
    {
        Player = player;
    }
}
