namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search.
/// </summary>
public abstract class LoadResult
{
    /// <summary>
    /// Gets the kind of load result.
    /// </summary>
    public LoadResultKind Kind { get; }

    internal LoadResult(LoadResultKind kind)
    {
        Kind = kind;
    }
}
