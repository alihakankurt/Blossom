using System.Collections.Immutable;
using Bloom.Playback;

namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search that has found tracks.
/// </summary>
public sealed class SearchLoadResult : LoadResult
{
    /// <summary>
    /// Gets the tracks found by the search.
    /// </summary>
    public ImmutableArray<BloomTrack> Tracks { get; init; }

    internal SearchLoadResult(ImmutableArray<BloomTrack> tracks) : base(LoadResultKind.Search)
    {
        Tracks = tracks;
    }
}
