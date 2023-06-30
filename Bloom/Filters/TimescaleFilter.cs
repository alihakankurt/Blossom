namespace Bloom.Filters;

/// <summary>
/// Changes the speed, pitch, and rate. All default to 1.0.
/// </summary>
public readonly struct TimescaleFilter : IFilter
{
    /// <summary>
    /// The playback speed (0.0 ≤).
    /// </summary>
    [JsonPropertyName("speed")]
    public float Speed { get; init; }

    /// <summary>
    /// The pitch (0.0 ≤).
    /// </summary>
    [JsonPropertyName("pitch")]
    public float Pitch { get; init; }

    /// <summary>
    /// The rate (0.0 ≤).
    /// </summary>
    [JsonPropertyName("rate")]
    public float Rate { get; init; }
}
