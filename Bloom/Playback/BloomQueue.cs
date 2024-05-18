using System;
using System.Collections;
using System.Collections.Generic;

namespace Bloom.Playback;

/// <summary>
/// Represents a <see cref="BloomTrack"/> queue for <see cref="BloomPlayer"/>.
/// </summary>
public sealed class BloomQueue : IEnumerable<BloomTrack>
{
    private BloomTrack[] _tracks;

    /// <summary>
    /// Gets the number of tracks in the queue.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the index of the current track in the queue.
    /// </summary>
    public int Current { get; private set; }

    /// <summary>
    /// Gets or sets the loop mode of the queue.
    /// </summary>
    public LoopMode LoopMode { get; set; }

    /// <summary>
    /// Gets the current track in the queue.
    /// </summary>
    public BloomTrack? CurrentTrack => (0 <= Current && Current < Count) ? _tracks[Current] : null;

    /// <summary>
    /// Gets a value indicating whether there is a next track in the queue.
    /// </summary>
    public bool HasNext => LoopMode is LoopMode.All || Current < Count - 1;

    /// <summary>
    /// Gets a value indicating whether there is a previous track in the queue.
    /// </summary>
    public bool HasPrevious => LoopMode is LoopMode.All || 0 < Current;

    /// <summary>
    /// Initializes a new instance of the <see cref="BloomQueue"/> class.
    /// </summary>
    public BloomQueue()
    {
        _tracks = [];
        Count = 0;
        Current = -1;
        LoopMode = LoopMode.None;
    }

    /// <summary>
    /// Adds a track to the queue.
    /// </summary>
    /// <param name="track">The <see cref="BloomTrack"/> to add to the queue.</param>
    public void Add(BloomTrack track)
    {
        EnsureCapacity(Count + 1);
        _tracks[Count] = track;
        Count += 1;
    }

    /// <summary>
    /// Adds a collection of tracks to the queue.
    /// </summary>
    /// <param name="tracks">The collection of <see cref="BloomTrack"/> to add to the queue.</param>
    public void Add(IReadOnlyList<BloomTrack> tracks)
    {
        EnsureCapacity(Count + tracks.Count);

        for (int trackIndex = 0; trackIndex < tracks.Count; trackIndex += 1)
            _tracks[Count + trackIndex] = tracks[trackIndex];

        Count += tracks.Count;
    }

    /// <summary>
    /// Inserts a track at the specified index in the queue.
    /// </summary>
    /// <param name="track">The <see cref="BloomTrack"/> to insert into the queue.</param>
    /// <param name="trackIndex">The index at which to insert the track.</param>
    public void InsertAt(BloomTrack track, int trackIndex)
    {
        EnsureTrackIndex(trackIndex);
        EnsureCapacity(Count + 1);

        for (int shiftIndex = Count; shiftIndex > trackIndex; shiftIndex -= 1)
            _tracks[shiftIndex] = _tracks[shiftIndex - 1];

        _tracks[trackIndex] = track;
        Count += 1;

        if (trackIndex <= Current)
            Current += 1;
    }

    /// <summary>
    /// Removes a track from the queue.
    /// </summary>
    /// <param name="track">The <see cref="BloomTrack"/> to remove from the queue.</param>
    /// <returns>Whether the track was successfully removed from the queue.</returns>
    public bool Remove(BloomTrack track)
    {
        int trackIndex = Array.IndexOf(_tracks, track);
        if (trackIndex == -1)
            return false;

        _ = RemoveAt(trackIndex);
        return true;
    }

    /// <summary>
    /// Removes a track at the specified index in the queue.
    /// </summary>
    /// <param name="trackIndex">The index of the track to remove from the queue.</param>
    /// <returns>The removed <see cref="BloomTrack"/>.</returns>
    public BloomTrack RemoveAt(int trackIndex)
    {
        EnsureTrackIndex(trackIndex);

        BloomTrack removed = _tracks[trackIndex];
        Count -= 1;

        for (int shiftIndex = trackIndex; shiftIndex < Count; shiftIndex++)
            _tracks[shiftIndex] = _tracks[shiftIndex + 1];

        if (trackIndex <= Current)
            Current -= 1;

        return removed;
    }

    /// <summary>
    /// Clears all tracks from the queue.
    /// </summary>
    public void Clear()
    {
        _tracks = [];
        Count = 0;
        Current = -1;
    }

    /// <summary>
    /// Determines whether the queue contains a specific track.
    /// </summary>
    /// <param name="track">The <see cref="BloomTrack"/> to locate in the queue.</param>
    /// <returns>Whether the track is found in the queue.</returns>
    public bool Contains(BloomTrack track)
    {
        return Array.IndexOf(_tracks, track) != -1;
    }

    /// <summary>
    /// Moves to the next track in the queue. If the end of the queue is reached, the behavior is determined by the <see cref="LoopMode"/>.
    /// </summary>
    public void MoveNext()
    {
        if (Count == 0 || LoopMode is LoopMode.One)
            return;

        Current += 1;
        if (Current >= Count)
            Current = (LoopMode is LoopMode.All) ? 0 : Count;
    }

    /// <summary>
    /// Moves to the previous track in the queue. If the beginning of the queue is reached, the behavior is determined by the <see cref="LoopMode"/>.
    /// </summary>
    public void MovePrevious()
    {
        if (Count == 0 || LoopMode is LoopMode.One)
            return;

        Current -= 1;
        if (Current < 0)
            Current = (LoopMode is LoopMode.All) ? Count - 1 : -1;
    }

    /// <summary>
    /// Shuffles the tracks in the queue.
    /// </summary>
    public void Shuffle()
    {
        if (Count <= 1)
            return;

        Random.Shared.Shuffle(_tracks.AsSpan(0, Current));
        Random.Shared.Shuffle(_tracks.AsSpan(Current + 1, Count - Current - 1));
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator that iterates through the queue.</returns>
    public IEnumerator<BloomTrack> GetEnumerator()
    {
        for (int trackIndex = 0; trackIndex < Count; trackIndex += 1)
            yield return _tracks[trackIndex];
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void EnsureCapacity(int capacity)
    {
        if (_tracks.Length >= capacity)
            return;

        Array.Resize(ref _tracks, capacity);
    }

    private void EnsureTrackIndex(int trackIndex)
    {
        if (0 <= trackIndex && trackIndex < Count)
            return;

        throw new IndexOutOfRangeException($"The {nameof(trackIndex)} was out of range of the queue");
    }
}
