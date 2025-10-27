namespace Bloom.Playback;

/// <summary>
/// Represents the state of a player.
/// </summary>
public enum PlayerState
{
    /// <summary>
    /// The player is not connected to a voice channel.
    /// </summary>
    Disconnected,

    /// <summary>
    /// The player is connected to a voice channel.
    /// </summary>
    Connected,

    /// <summary>
    /// The player is playing a track.
    /// </summary>
    Playing,

    /// <summary>
    /// The player is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The player is stopped and the queue is empty.
    /// </summary>
    Stopped,
}
