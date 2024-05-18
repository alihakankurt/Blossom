using System;

namespace Bloom.Searching;

/// <summary>
/// Represents the kind of search to perform.
/// </summary>
public enum SearchKind
{
    /// <summary>
    /// A direct search with a well-known URL.
    /// </summary>
    Direct,

    /// <summary>
    /// A search on YouTube.
    /// </summary>
    YouTube,

    /// <summary>
    /// A search on YouTube Music.
    /// </summary>
    YouTubeMusic,

    /// <summary>
    /// A search on SoundCloud.
    /// </summary>
    SoundCloud,
}

/// <summary>
/// Provides extension methods for <see cref="SearchKind"/>.
/// </summary>
public static class SearchKindExtensions
{
    /// <summary>
    /// Wraps the query with the search kind.
    /// </summary>
    /// <param name="kind">The search kind.</param>
    /// <param name="query">The original query.</param>
    /// <returns>The wrapped query.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string WrapQuery(this SearchKind kind, string query) => kind switch
    {
        SearchKind.Direct => query,
        SearchKind.YouTube => $"ytsearch:{query}",
        SearchKind.YouTubeMusic => $"ytmsearch:{query}",
        SearchKind.SoundCloud => $"scsearch:{query}",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
