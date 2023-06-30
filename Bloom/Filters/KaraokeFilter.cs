namespace Bloom.Filters;

/// <summary>
/// Uses equalization to eliminate part of a band, usually targeting vocals.
/// </summary>
public readonly struct KaraokeFilter : IFilter
{
    /// <summary>
    /// The level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect).
    /// </summary>
    [JsonPropertyName("level")]
    public float Level { get; init; }

    /// <summary>
    /// The mono level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect).
    /// </summary>
    [JsonPropertyName("monoLevel")]
    public float MonoLevel { get; init; }

    /// <summary>
    /// The filter band.
    /// </summary>
    [JsonPropertyName("filterBand")]
    public float FilterBand { get; init; }

    /// <summary>
    /// The filter width.
    /// </summary>
    [JsonPropertyName("filterWidth")]
    public float FilterWidth { get; init; }
}
