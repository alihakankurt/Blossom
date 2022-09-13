namespace Blossom.Models;

public struct AudioFilter
{
    public List<IFilter> Filters;
    public EqualizerBand[] Bands;

    public AudioFilter()
    {
        Filters = new List<IFilter> { new EmptyFilter() };
        Bands = new EqualizerBand[15];
        for (int i = 0; i < 15; i++)
        {
            Bands[i] = new EqualizerBand(i, 0);
        }
    }

    public AudioFilter(params double[] gains) : this()
    {
        for (int i = 0; i < gains.Length; i++)
        {
            Bands[i] = new EqualizerBand(i, gains[i]);
        }
    }

    public AudioFilter(IFilter filter, params double[] gains) : this(gains)
    {
        Filters[0] = filter;
    }

    public AudioFilter(List<IFilter> filters, params double[] gains) : this(gains)
    {
        Filters = filters;
    }

    public class EmptyFilter : IFilter
    {
    }
}
