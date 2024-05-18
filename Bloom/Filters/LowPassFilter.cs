namespace Bloom.Filters;

/// <summary>
/// Higher frequencies get suppressed, while lower frequencies pass through this filter, thus the name low pass.
/// Any smoothing values less than or equal to 1.0 will disable the filter.
/// </summary>
public sealed class LowPassFilter : IFilter
{
    /// <summary>
    /// The smoothing factor.
    /// </summary>
    public float Smoothing { get; init; }
}
