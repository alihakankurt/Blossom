namespace Bloom.Filters;

/// <summary>
/// Similar to tremolo. While tremolo oscillates the volume, vibrato oscillates the pitch.
/// </summary>
public readonly struct VibratoFilter : IFilter
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
