using System.Collections.Generic;

namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search that has found tracks.
/// </summary>
public sealed class SearchLoadResult : LoadResult
{
    /// <summary>
    /// Gets the tracks found by the search.
    /// </summary>
    public IReadOnlyList<BloomTrack> Tracks { get; init; }

    internal SearchLoadResult(IReadOnlyList<BloomTrack> tracks) : base(LoadResultKind.Search)
    {
        Tracks = tracks;
    }
}
