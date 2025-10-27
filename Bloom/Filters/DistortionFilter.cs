namespace Bloom.Filters;

/// <summary>
/// Distortion effect. It can generate some pretty unique audio effects.
/// </summary>
public sealed class DistortionFilter : IFilter
{
    /// <summary>
    /// The sin offset.
    /// </summary>
    public float SinOffset { get; init; }

    /// <summary>
    /// The sin scale.
    /// </summary>
    public float SinScale { get; init; }

    /// <summary>
    /// The cos offset.
    /// </summary>
    public float CosOffset { get; init; }

    /// <summary>
    /// The cos scale.
    /// </summary>
    public float CosScale { get; init; }

    /// <summary>
    /// The tan offset.
    /// </summary>
    public float TanOffset { get; init; }

    /// <summary>
    /// The tan scale.
    /// </summary>
    public float TanScale { get; init; }

    /// <summary>
    /// The offset.
    /// </summary>
    public float Offset { get; init; }

    /// <summary>
    /// The scale.
    /// </summary>
    public float Scale { get; init; }

}
