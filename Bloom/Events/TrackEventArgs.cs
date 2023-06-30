namespace Bloom.Events;

public abstract class TrackEventArgs
{
    public BloomPlayer Player { get; }

    internal TrackEventArgs(BloomPlayer player)
    {
        Player = player;
    }
}
