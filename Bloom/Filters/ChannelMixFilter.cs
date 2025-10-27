namespace Bloom.Filters;

/// <summary>
/// Mixes both channels (left and right), with a configurable factor <i>(0.0 to 1.0)</i> on how much each channel affects the other.
/// With the defaults, both channels are kept independent of each other. Setting all factors to 0.5 means both channels get the same audio.
/// </summary>
public sealed class ChannelMixFilter : IFilter
{
    /// <summary>
    /// The left to left channel mix factor.
    /// </summary>
    public float LeftToLeft { get; init; }

    /// <summary>
    /// The left to right channel mix factor.
    /// </summary>
    public float LeftToRight { get; init; }

    /// <summary>
    /// The right to left channel mix factor.
    /// </summary>
    public float RightToLeft { get; init; }

    /// <summary>
    /// The right to right channel mix factor.
    /// </summary>
    public float RightToRight { get; init; }
}
