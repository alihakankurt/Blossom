using System.Collections.Generic;

namespace Bloom.Filters;

/// <summary>
/// Represents a filter preset with a set of equalizer bands and filters.
/// </summary>
public sealed class FilterPreset
{
    /// <summary>
    /// Resets all bands to 0 and removes all filters.
    /// </summary>
    public static readonly FilterPreset Flat;

    /// <summary>
    /// Boosts the lower frequencies.
    /// </summary>
    public static readonly FilterPreset Bass;

    /// <summary>
    /// Boosts the mid frequencies.
    /// </summary>
    public static readonly FilterPreset Classical;

    /// <summary>
    /// Boosts the high frequencies.
    /// </summary>
    public static readonly FilterPreset Electronic;

    /// <summary>
    /// Boosts the mid frequencies and cuts the high frequencies.
    /// </summary>
    public static readonly FilterPreset Rock;

    /// <summary>
    /// Applies a low-pass filter.
    /// </summary>
    public static readonly FilterPreset Soft;

    /// <summary>
    /// Rotates the audio around the listener.
    /// </summary>
    public static readonly FilterPreset Rotation;

    /// <summary>
    /// Speeds up the audio and boosts the pitch.
    /// </summary>
    public static readonly FilterPreset Nightcore;

    /// <summary>
    /// Speeds up the audio and boosts the pitch slightly.
    /// </summary>
    public static readonly FilterPreset LoveNightcore;

    /// <summary>
    /// Applies a tremolo effect.
    /// </summary>
    public static readonly FilterPreset Tremolo;

    /// <summary>
    /// Applies a vibrato effect.
    /// </summary>
    public static readonly FilterPreset Vibrato;

    /// <summary>
    /// The list of all filter presets.
    /// </summary>
    public static readonly FilterPreset[] Presets;

    static FilterPreset()
    {
        Flat = new FilterPreset
        {
            Name = nameof(Flat),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = []
        };

        Bass = new FilterPreset
        {
            Name = nameof(Bass),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.6f), new(1, 0.7f), new(2, 0.8f), new(3, 0.55f), new(4, 0.25f),
                new(5, 0.0f), new(6, -0.25f), new(7, -0.25f), new(8, -0.25f), new(9, -0.25f),
                new(10, -0.25f), new(11, -0.25f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = []
        };

        Classical = new FilterPreset
        {
            Name = nameof(Classical),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.375f), new(1, 0.35f), new(2, 0.125f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.125f), new(6, 0.55f), new(7, 0.5f), new(8, 0.125f), new(9, 0.25f),
                new(10, 0.2f), new(11, 0.25f), new(12, 0.3f), new(13, 0.25f), new(14, 0.3f),
            ],
            Filters = []
        };

        Electronic = new FilterPreset
        {
            Name = nameof(Electronic),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.375f), new(1, 0.35f), new(2, 0.125f), new(3, 0.0f), new(4, 0.0f),
                new(5, -0.125f), new(6, -0.125f), new(7, 0.0f), new(8, 0.25f), new(9, 0.125f),
                new(10, 0.15f), new(11, 0.2f), new(12, 0.25f), new(13, 0.35f), new(14, 0.4f),
            ],
            Filters = []
        };

        Rock = new FilterPreset
        {
            Name = nameof(Rock),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.3f), new(1, 0.25f), new(2, 0.2f), new(3, 0.1f), new(4, 0.05f),
                new(5, -0.05f), new(6, -0.15f), new(7, -0.2f), new(8, -0.1f), new(9, -0.05f),
                new(10, 0.05f), new(11, 0.1f), new(12, 0.2f), new(13, 0.25f), new(14, 0.3f),
            ],
            Filters = []
        };

        Soft = new FilterPreset
        {
            Name = nameof(Soft),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = [new LowPassFilter { Smoothing = 20.0f }]
        };

        Rotation = new FilterPreset
        {
            Name = nameof(Rotation),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = [new RotationFilter { RotationHz = 0.2f }]
        };

        Nightcore = new FilterPreset
        {
            Name = nameof(Nightcore),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = [new TimescaleFilter { Speed = 1.3f, Pitch = 1.3f, Rate = 1.0f }]
        };

        LoveNightcore = new FilterPreset
        {
            Name = nameof(LoveNightcore),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = [new TimescaleFilter { Speed = 1.1f, Pitch = 1.2f, Rate = 1.0f }]
        };

        Tremolo = new FilterPreset
        {
            Name = nameof(Tremolo),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = [new TremoloFilter { Frequency = 10.0f, Depth = 0.5f }]
        };

        Vibrato = new FilterPreset
        {
            Name = nameof(Vibrato),
            Volume = 1.0f,
            Equalizer = [
                new(0, 0.0f), new(1, 0.0f), new(2, 0.0f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.0f), new(6, 0.0f), new(7, 0.0f), new(8, 0.0f), new(9, 0.0f),
                new(10, 0.0f), new(11, 0.0f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            ],
            Filters = [new VibratoFilter { Frequency = 10.0f, Depth = 0.9f }]
        };

        Presets = [Flat, Bass, Classical, Electronic, Rock, Soft, Rotation, Nightcore, LoveNightcore, Tremolo, Vibrato];
    }

    /// <summary>
    /// The name of the preset.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The volume of the preset.
    /// </summary>
    public required float Volume { get; init; }

    /// <summary>
    /// The equalizer bands in the preset.
    /// </summary>
    public required EqualizerBand[] Equalizer { get; init; }

    /// <summary>
    /// The filters in the preset.
    /// </summary>
    public required IReadOnlyList<IFilter> Filters { get; init; }
}
