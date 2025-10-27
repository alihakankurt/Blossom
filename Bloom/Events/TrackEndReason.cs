namespace Bloom.Events;

/// <summary>
/// Represents the reason a track has ended.
/// </summary>
public enum TrackEndReason
{
    /// <summary>
    /// The track has finished playing. May start the next track.
    /// </summary>
    Finished,

    /// <summary>
    /// The track failed to load. May start the next track.
    /// </summary>
    LoadFailed,

    /// <summary>
    /// The track was stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The track was replaced by another track.
    /// </summary>
    Replaced,

    /// <summary>
    /// The track was cleaned up.
    /// </summary>
    Cleanup,
}
