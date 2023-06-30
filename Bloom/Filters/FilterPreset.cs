namespace Bloom.Filters;

public sealed class FilterPreset
{
    public static readonly FilterPreset Flat;
    public static readonly FilterPreset Bass;
    public static readonly FilterPreset Classical;
    public static readonly FilterPreset Electronic;
    public static readonly FilterPreset Rock;
    public static readonly FilterPreset Soft;
    public static readonly FilterPreset EightDimensional;
    public static readonly FilterPreset Nightcore;
    public static readonly FilterPreset LoveNightcore;
    public static readonly FilterPreset Tremolo;
    public static readonly FilterPreset Vibrato;
    public static readonly FilterPreset[] Presets;

    public string Name { get; }
    public float Volume { get; }
    public EqualizerBand[] Bands { get; }
    public IEnumerable<IFilter> Filters { get; }

    static FilterPreset()
    {
        Flat = new FilterPreset(nameof(Flat), 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray()
        );
        Bass = new FilterPreset(nameof(Bass), 1.0f,
            new EqualizerBand[]
            {
                new(0, 0.6f), new(1, 0.7f), new(2, 0.8f), new(3, 0.55f), new(4, 0.25f),
                new(5, 0.0f), new(6, -0.25f), new(7, -0.25f), new(8, -0.25f), new(9, -0.25f),
                new(10, -0.25f), new(11, -0.25f), new(12, 0.0f), new(13, 0.0f), new(14, 0.0f),
            }
        );
        Classical = new FilterPreset(nameof(Classical), 1.0f,
            new EqualizerBand[]
            {
                new(0, 0.375f), new(1, 0.35f), new(2, 0.125f), new(3, 0.0f), new(4, 0.0f),
                new(5, 0.125f), new(6, 0.55f), new(7, 0.5f), new(8, 0.125f), new(9, 0.25f),
                new(10, 0.2f), new(11, 0.25f), new(12, 0.3f), new(13, 0.25f), new(14, 0.3f),
            }
        );
        Electronic = new FilterPreset(nameof(Electronic), 1.0f,
            new EqualizerBand[]
            {
                new(0, 0.375f), new(1, 0.35f), new(2, 0.125f), new(3, 0.0f), new(4, 0.0f),
                new(5, -0.125f), new(6, -0.125f), new(7, 0.0f), new(8, 0.25f), new(9, 0.125f),
                new(10, 0.15f), new(11, 0.2f), new(12, 0.25f), new(13, 0.35f), new(14, 0.4f),
            }
        );
        Rock = new FilterPreset(nameof(Rock), 1.0f,
            new EqualizerBand[]
            {
                new(0, 0.3f), new(1, 0.25f), new(2, 0.2f), new(3, 0.1f), new(4, 0.05f),
                new(5, -0.05f), new(6, -0.15f), new(7, -0.2f), new(8, -0.1f), new(9, -0.05f),
                new(10, 0.05f), new(11, 0.1f), new(12, 0.2f), new(13, 0.25f), new(14, 0.3f),
            }
        );
        Soft = new FilterPreset(nameof(Soft), 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray(),
            new LowPassFilter { Smoothing = 20.0f }
        );
        EightDimensional = new FilterPreset("8D", 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray(),
            new RotationFilter { RotationHz = 0.2f }
        );
        Nightcore = new FilterPreset(nameof(Nightcore), 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray(),
            new TimescaleFilter { Speed = 1.3f, Pitch = 1.3f, Rate = 1.0f }
        );
        LoveNightcore = new FilterPreset(nameof(LoveNightcore), 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray(),
            new TimescaleFilter { Speed = 1.1f, Pitch = 1.2f, Rate = 1.0f }
        );
        Tremolo = new FilterPreset(nameof(Tremolo), 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray(),
            new TremoloFilter { Frequency = 10.0f, Depth = 0.5f }
        );
        Vibrato = new FilterPreset(nameof(Vibrato), 1.0f,
            Enumerable.Range(0, 15)
                .Select((index) => new EqualizerBand(index, 0.0f))
                .ToArray(),
            new VibratoFilter { Frequency = 10.0f, Depth = 0.9f }
        );


        Presets = new[]
        {
            Flat,
            Bass,
            Classical,
            Electronic,
            Rock,
            Soft,
            EightDimensional,
            Nightcore,
            LoveNightcore,
            Tremolo,
            Vibrato,
        };
    }

    public FilterPreset(string name, float volume, EqualizerBand[] bands, params IFilter[] filters)
    {
        Name = name;
        Volume = volume;
        Bands = bands;
        Filters = filters;
    }
}
