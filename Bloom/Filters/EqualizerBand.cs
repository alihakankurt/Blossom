using System;

namespace Bloom.Filters;

/// <summary>
/// Represents a single band in an equalizer filter.
/// <para>
/// The band frequencies are as follows:
/// <list type="table">
/// <item>
/// | <term>0</term><description> 25 Hz</description>
/// | <term>1</term><description> 40 Hz</description>
/// | <term>2</term><description> 63 Hz</description>
/// </item>
/// <item>
/// | <term>3</term><description> 100 Hz</description>
/// | <term>4</term><description> 160 Hz</description>
/// | <term>5</term><description> 250 Hz</description>
/// </item>
/// <item>
/// | <term>6</term><description> 400 Hz</description>
/// | <term>7</term><description> 630 Hz</description>
/// | <term>8</term><description> 1000 Hz</description>
/// </item>
/// <item>
/// | <term>9</term><description> 1600 Hz</description>
/// | <term>10</term><description> 2500 Hz</description>
/// | <term>11</term><description> 4000 Hz</description>
/// </item>
/// <item>
/// | <term>12</term><description> 6300 Hz</description>
/// | <term>13</term><description> 10000 Hz</description>
/// | <term>14</term><description> 16000 Hz</description>
/// </item>
/// </list>
/// </para>
/// </summary>
public readonly struct EqualizerBand
{
    /// <summary>
    /// The minimum band index.
    /// </summary>
    public const int MinBand = 0;

    /// <summary>
    /// The maximum band index.
    /// </summary>
    public const int MaxBand = 14;

    /// <summary>
    /// The minimum gain value.
    /// </summary>
    public const float MinGain = -0.25f;

    /// <summary>
    /// The maximum gain value.
    /// </summary>
    public const float MaxGain = 1.00f;

    /// <summary>
    /// The band index.
    /// </summary>
    public readonly int Band { get; }

    /// <summary>
    /// The gain value.
    /// </summary>
    public readonly float Gain { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EqualizerBand"/> struct.
    /// </summary>
    /// <param name="band">The band index.</param>
    /// <param name="gain">The gain value.</param>
    public EqualizerBand(int band, float gain)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(band, MinBand);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(band, MaxBand);

        ArgumentOutOfRangeException.ThrowIfLessThan(gain, MinGain);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(gain, MaxGain);

        Band = band;
        Gain = gain;
    }
}
