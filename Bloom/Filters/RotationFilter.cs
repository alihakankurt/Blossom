namespace Bloom.Filters;

/// <summary>
/// Rotates the sound around the stereo channels/user headphones aka Audio Panning. It can produce an effect similar to.
/// </summary>
public readonly struct RotationFilter : IFilter
{
    /// <summary>
    /// The frequency of the audio rotating around the listener in Hz.
    /// </summary>
    [JsonPropertyName("rotationHz")]
    public float RotationHz { get; init; }
}
