namespace Bloom.Filters;

public readonly struct EqualizerBand
{
    [JsonPropertyName("band")]
    public int Band { get; }

    [JsonPropertyName("gain")]
    public float Gain { get; }

    public EqualizerBand(int band, float gain)
    {
        Band = band is < 0 or > 14
            ? throw new ArgumentOutOfRangeException(nameof(band), "There are only 15 bands starting as zero-indexed.")
            : band;

        Gain = gain is < -0.25f or > 1.0f
            ? throw new ArgumentOutOfRangeException(nameof(gain), "The gain takes a value between -0.25 and 1.0.")
            : gain;
    }
}
