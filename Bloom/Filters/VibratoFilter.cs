namespace Bloom.Filters;

/// <summary>
/// Uses amplification to create a warbling effect, where the pitch quickly oscillates.
/// </summary>
public sealed class VibratoFilter : IFilter
{
    /// <summary>
    /// The frequency.
    /// </summary>
    public float Frequency { get; init; }

    /// <summary>
    /// The depth.
    /// </summary>
    public float Depth { get; init; }
}
