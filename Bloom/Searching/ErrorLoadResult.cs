namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search that has failed.
/// </summary>
public sealed class ErrorLoadResult : LoadResult
{
    /// <summary>
    /// Gets the exception that caused the search to fail.
    /// </summary>
    public BloomException Exception { get; }

    internal ErrorLoadResult(BloomException exception) : base(LoadResultKind.Error)
    {
        Exception = exception;
    }
}
