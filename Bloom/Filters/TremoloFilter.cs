namespace Bloom.Filters;

/// <summary>
/// Uses amplification to create a shuddering effect, where the volume quickly oscillates.
/// </summary>
public readonly struct TremoloFilter : IFilter
{
    /// <summary>
    /// The frequency (0.0 ≤).
    /// </summary>
    [JsonPropertyName("frequency")]
    public float Frequency { get; init; }

    /// <summary>
    /// The depth (0.0 ≤).
    /// </summary>
    [JsonPropertyName("depth")]
    public float Depth { get; init; }
}
