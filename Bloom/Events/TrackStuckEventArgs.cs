using System;
using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Provides data for the <see cref="TrackStuckEvent"/> event.
/// </summary>
public sealed class TrackStuckEventArgs : TrackEventArgs
{
    /// <summary>
    /// Gets the threshold that was exceeded.
    /// </summary>
    public TimeSpan Threshold { get; }

    internal TrackStuckEventArgs(BloomPlayer player, TimeSpan threshold) : base(player)
    {
        Threshold = threshold;
    }
}
