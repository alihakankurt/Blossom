namespace Bloom.Filters;

/// <summary>
/// Changes the speed, pitch, and rate. All default to 1.0.
/// </summary>
public sealed class TimescaleFilter : IFilter
{
    /// <summary>
    /// The playback speed.
    /// </summary>
    public float Speed { get; init; }

    /// <summary>
    /// The pitch.
    /// </summary>
    public float Pitch { get; init; }

    /// <summary>
    /// The rate.
    /// </summary>
    public float Rate { get; init; }
}
