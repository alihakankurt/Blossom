namespace Bloom.Filters;

/// <summary>
/// Uses amplification to create a shuddering effect, where the volume quickly oscillates.
/// </summary>
public sealed class TremoloFilter : IFilter
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
