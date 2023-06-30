using System.Collections;

namespace Bloom;

public sealed class BloomQueue : ICollection<BloomTrack>, IAsyncDisposable
{
    private BloomTrack[] _tracks;
    private int _count;
    private int _current;
    private LoopMode _loopMode;

    public bool IsReadOnly => false;
    public bool IsEmpty => _count is 0;
    public int Count => _count;
    public int Current => _current;
    public BloomTrack? CurrentTrack => (0 <= _current && _current < _count) ? _tracks[_current] : null;
    public LoopMode LoopMode => _loopMode;
    public bool HasNext => _loopMode is LoopMode.All || _current < _count - 1;
    public bool HasPrevious => _loopMode is LoopMode.All || 0 < _current;

    public BloomQueue()
    {
        _tracks = Array.Empty<BloomTrack>();
        _count = 0;
        _current = -1;
        _loopMode = LoopMode.None;
    }

    public ValueTask DisposeAsync()
    {
        Clear();
        Array.Clear(_tracks);
        _tracks = null;
        return ValueTask.CompletedTask;
    }

    public void Add(BloomTrack track)
    {
        if (_count >= _tracks.Length)
            Array.Resize(ref _tracks, _tracks.Length + 4);

        _tracks[_count++] = track;
    }

    public void AddRange(IEnumerable<BloomTrack> tracks)
    {
        foreach (BloomTrack track in tracks)
            Add(track);
    }

    public void InsertAt(BloomTrack track, int trackIndex)
    {
        if (_count >= _tracks.Length)
            Array.Resize(ref _tracks, _tracks.Length + 4);

        _count++;
        for (int shiftIndex = _count - 1; shiftIndex > trackIndex; shiftIndex--)
            _tracks[shiftIndex] = _tracks[shiftIndex - 1];

        _tracks[trackIndex] = track;

        if (trackIndex <= _current)
            _current++;
    }

    public bool Remove(BloomTrack track)
    {
        for (int trackIndex = 0; trackIndex < _count; trackIndex++)
        {
            if (track.Identifier == _tracks[trackIndex].Identifier)
            {
                RemoveAt(trackIndex);
                return true;
            }
        }

        return false;
    }

    public BloomTrack RemoveAt(int trackIndex)
    {
        if (0 > trackIndex && trackIndex >= _count)
            throw new IndexOutOfRangeException($"The {nameof(trackIndex)} was out of range of the queue");

        BloomTrack removed = _tracks[trackIndex];

        for (int shiftIndex = trackIndex + 1; shiftIndex < _count; shiftIndex++)
            _tracks[shiftIndex - 1] = _tracks[shiftIndex];

        Array.Resize(ref _tracks, --_count);
        if (trackIndex <= _current)
            _current--;

        return removed;
    }

    public void Clear()
    {
        _tracks = Array.Empty<BloomTrack>();
        _count = 0;
        _current = -1;
    }

    public bool Contains(BloomTrack track)
    {
        return _tracks.Any((t) => t.Identifier == track.Identifier);
    }

    public void Next()
    {
        if (IsEmpty || _loopMode is LoopMode.One)
            return;

        if (_loopMode is LoopMode.All)
        {
            _current = (_current + 1) % _count;
            return;
        }

        _current = Math.Min(_current + 1, _count);
    }

    public void Previous()
    {
        if (IsEmpty || _loopMode is LoopMode.One)
            return;

        if (_loopMode is LoopMode.All)
        {
            _current = (_current - 1 + _count) % _count;
            return;
        }

        _current = Math.Max(_current - 1, -1);
    }

    public void Loop()
    {
        _loopMode = _loopMode switch
        {
            LoopMode.None => LoopMode.One,
            LoopMode.One => LoopMode.All,
            _ => LoopMode.None,
        };
    }

    public void Shuffle()
    {
        if (_count is ( <= 1))
            return;

        IList<BloomTrack> previous = _tracks.Take(0.._current).ToList().Shuffle();
        IList<BloomTrack> next = _tracks.Take((_current + 1).._count).ToList().Shuffle();
        _tracks = previous.Append(CurrentTrack!).Concat(next).ToArray();
    }

    public void CopyTo(BloomTrack[] array, int arrayIndex)
    {
        _tracks.CopyTo(array, arrayIndex);
    }

    public IEnumerator<BloomTrack> GetEnumerator()
    {
        foreach (BloomTrack track in _tracks)
            yield return track;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
