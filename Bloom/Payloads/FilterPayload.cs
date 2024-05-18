using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bloom.Filters;

namespace Bloom.Payloads;

internal sealed class FilterPayload
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public float Volume { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EqualizerBand[] Equalizer { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public KaraokeFilter? Karaoke { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TimescaleFilter? Timescale { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TremoloFilter? Tremolo { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public VibratoFilter? Vibrato { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public RotationFilter? Rotation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DistortionFilter? Distortion { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ChannelMixFilter? ChannelMix { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public LowPassFilter? LowPass { get; set; }

    public FilterPayload(IFilter filter, float volume, EqualizerBand[] equalizer)
    {
        Volume = volume;
        Equalizer = equalizer;
        SetFilter(filter);
    }

    public FilterPayload(IReadOnlyList<IFilter> filters, float volume, EqualizerBand[] equalizer)
    {
        Volume = volume;
        Equalizer = equalizer;
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
