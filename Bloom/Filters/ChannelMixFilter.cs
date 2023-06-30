namespace Bloom.Filters;

/// <summary>
/// Mixes both channels (left and right), with a configurable factor on how much each channel affects the other.
/// With the defaults, both channels are kept independent of each other. Setting all factors to 0.5 means both channels get the same audio.
/// </summary>
public readonly struct ChannelMixFilter : IFilter
{
    /// <summary>
    /// The left to left channel mix factor (>= 0.0 and ≤ 1.0).
    /// </summary>
    [JsonPropertyName("leftToLeft")]
    public float LeftToLeft { get; init; }

    /// <summary>
    /// The left to right channel mix factor (>= 0.0 and ≤ 1.0).
    /// </summary>
    [JsonPropertyName("leftToRight")]
    public float LeftToRight { get; init; }

    /// <summary>
    /// The right to left channel mix factor (>= 0.0 and ≤ 1.0).
    /// </summary>
    [JsonPropertyName("rightToLeft")]
    public float RightToLeft { get; init; }

    /// <summary>
    /// The right to right channel mix factor (>= 0.0 and ≤ 1.0).
    /// </summary>
    [JsonPropertyName("rightToRight")]
    public float RightToRight { get; init; }
}
