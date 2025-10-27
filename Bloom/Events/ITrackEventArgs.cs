using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Provides data for an event that is related to a track.
/// </summary>
public interface ITrackEventArgs
{
    /// <summary>
    /// Gets the <see cref="BloomPlayer"/> instance that the event is related to.
    /// </summary>
    public BloomPlayer Player { get; }
}
