namespace Bloom.Filters;

/// <summary>
/// Rotates the sound around the stereo channels/user headphones (aka Audio Panning).
/// </summary>
public sealed class RotationFilter : IFilter
{
    /// <summary>
    /// The frequency of the audio rotating around the listener in Hz.
    /// </summary>
    public float RotationHz { get; init; }
}
