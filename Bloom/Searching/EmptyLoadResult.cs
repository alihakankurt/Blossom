namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search that has no results.
/// </summary>
public sealed class EmptyLoadResult : LoadResult
{
    internal EmptyLoadResult() : base(LoadResultKind.Empty)
    {
    }
}
