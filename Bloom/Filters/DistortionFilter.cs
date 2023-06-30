namespace Bloom.Filters;

/// <summary>
/// Distortion effect. It can generate some pretty unique audio effects.
/// </summary>
public readonly struct DistortionFilter : IFilter
{
    /// <summary>
    /// The sin offset.
    /// </summary>
    [JsonPropertyName("sinOffset")]
    public float SinOffset { get; init; }

    /// <summary>
    /// The sin scale.
    /// </summary>
    [JsonPropertyName("sinScale")]
    public float SinScale { get; init; }

    /// <summary>
    /// The cos offset.
    /// </summary>
    [JsonPropertyName("cosOffset")]
    public float CosOffset { get; init; }

    /// <summary>
    /// The cos scale.
    /// </summary>
    [JsonPropertyName("cosScale")]
    public float CosScale { get; init; }

    /// <summary>
    /// The tan offset.
    /// </summary>
    [JsonPropertyName("tanOffset")]
    public float TanOffset { get; init; }

    /// <summary>
    /// The tan scale.
    /// </summary>
    [JsonPropertyName("tanScale")]
    public float TanScale { get; init; }

    /// <summary>
    /// The offset.
    /// </summary>
    [JsonPropertyName("offset")]
    public float Offset { get; init; }

    /// <summary>
    /// The scale.
    /// </summary>
    [JsonPropertyName("scale")]
    public float Scale { get; init; }

}
