using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Represents an asynchronous event handler that is called when current track caused an exception.
/// </summary>
/// <param name="args">The <see cref="TrackExceptionEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task TrackExceptionEvent(TrackExceptionEventArgs args);

/// <summary>
/// Provides data for the <see cref="TrackExceptionEvent"/>.
/// </summary>
public readonly struct TrackExceptionEventArgs : ITrackEventArgs
{
    /// <inheritdoc/>
    public readonly BloomPlayer Player { get; }

    /// <summary>
    /// Gets the <see cref="BloomException"/> that was thrown.
    /// </summary>
    public readonly BloomException Exception { get; }

    internal TrackExceptionEventArgs(BloomPlayer player, BloomException exception)
    {
        Player = player;
        Exception = exception;
    }
}
