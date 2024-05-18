namespace Bloom.Filters;

/// <summary>
/// Uses equalization to eliminate part of a band, usually targeting vocals.
/// </summary>
public sealed class KaraokeFilter : IFilter
{
    /// <summary>
    /// The level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect).
    /// </summary>
    public float Level { get; init; }

    /// <summary>
    /// The mono level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect).
    /// </summary>
    public float MonoLevel { get; init; }

    /// <summary>
    /// The filter band in frequency in Hz.
    /// </summary>
    public float FilterBand { get; init; }

    /// <summary>
    /// The filter width.
    /// </summary>
    public float FilterWidth { get; init; }
}
