namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search that has found a track.
/// </summary>
public sealed class TrackLoadResult : LoadResult
{
    /// <summary>
    /// Gets the track that was found.
    /// </summary>
    public BloomTrack Track { get; init; }

    internal TrackLoadResult(BloomTrack track) : base(LoadResultKind.Track)
    {
        Track = track;
    }
}
