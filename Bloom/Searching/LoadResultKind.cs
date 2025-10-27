namespace Bloom.Searching;

/// <summary>
/// Represents the kind of a load result.
/// </summary>
public enum LoadResultKind
{
    /// <summary>
    /// No matches were found.
    /// </summary>
    Empty,

    /// <summary>
    /// An error occurred.
    /// </summary>
    Error,

    /// <summary>
    /// A search result.
    /// </summary>
    Search,

    /// <summary>
    /// A track were found.
    /// </summary>
    Track,

    /// <summary>
    /// A playlist were found.
    /// </summary>
    Playlist,
}
