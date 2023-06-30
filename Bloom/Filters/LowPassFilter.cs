namespace Bloom.Filters;

/// <summary>
/// Higher frequencies get suppressed, while lower frequencies pass through this filter, thus the name low pass.
/// Any smoothing values equal to, or less than 1.0 will disable the filter.
/// </summary>
public readonly struct LowPassFilter : IFilter
{
    /// <summary>
    /// The smoothing factor (1.0 < )
    /// </summary>
    [JsonPropertyName("smoothing")]
    public float Smoothing { get; init; }
}
