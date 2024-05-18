using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Provides data for the <see cref="TrackExceptionEvent"/> event.
/// </summary>
public sealed class TrackExceptionEventArgs : TrackEventArgs
{
    /// <summary>
    /// Gets the <see cref="BloomException"/> that was thrown.
    /// </summary>
    public BloomException Exception { get; }

    internal TrackExceptionEventArgs(BloomPlayer player, BloomException exception) : base(player)
    {
        Exception = exception;
    }
}
