namespace Bloom.Payloads;

internal sealed class FilterPayload : IPayload
{
    [JsonPropertyName("volume"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public float Volume { get; }

    [JsonPropertyName("equalizer"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<EqualizerBand> Bands { get; }

    [JsonPropertyName("karaoke"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public KaraokeFilter Karaoke { get; set; }

    [JsonPropertyName("timescale"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TimescaleFilter Timescale { get; set; }

    [JsonPropertyName("tremolo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TremoloFilter Tremolo { get; set; }

    [JsonPropertyName("vibrato"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public VibratoFilter Vibrato { get; set; }

    [JsonPropertyName("rotation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public RotationFilter Rotation { get; set; }

    [JsonPropertyName("distortion"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DistortionFilter Distortion { get; set; }

    [JsonPropertyName("channelMix"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ChannelMixFilter ChannelMix { get; set; }

    [JsonPropertyName("lowPass"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public LowPassFilter LowPass { get; set; }

    public FilterPayload(IFilter filter, float volume, IEnumerable<EqualizerBand> bands)
    {
        Volume = volume;
        Bands = bands;
        SetFilter(filter);
    }

    public FilterPayload(IEnumerable<IFilter> filters, float volume, IEnumerable<EqualizerBand> bands)
    {
        Volume = volume;
        Bands = bands;
        foreach (IFilter filter in filters)
            SetFilter(filter);
    }

    private void SetFilter(IFilter filter)
    {
        switch (filter)
        {
            case KaraokeFilter karaokeFilter:
                Karaoke = karaokeFilter;
                break;

            case TimescaleFilter timescaleFilter:
                Timescale = timescaleFilter;
                break;

            case TremoloFilter tremoloFilter:
                Tremolo = tremoloFilter;
                break;

            case VibratoFilter vibratoFilter:
                Vibrato = vibratoFilter;
                break;

            case RotationFilter rotationFilter:
                Rotation = rotationFilter;
                break;

            case DistortionFilter distortionFilter:
                Distortion = distortionFilter;
                break;

            case ChannelMixFilter channelMixFilter:
                ChannelMix = channelMixFilter;
                break;

            case LowPassFilter lowPassFilter:
                LowPass = lowPassFilter;
                break;
        }
    }
}
