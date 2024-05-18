using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Provides data for the <see cref="TrackStartEvent"/> event.
/// </summary>
public sealed class TrackStartEventArgs : TrackEventArgs
{
    internal TrackStartEventArgs(BloomPlayer player) : base(player)
    {
    }
}
