using System.Collections.Generic;

namespace Bloom.Searching;

/// <summary>
/// Represents the result of a search that has found a playlist.
/// </summary>
public sealed class PlaylistLoadResult : LoadResult
{
    /// <summary>
    /// Gets the name of the playlist.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the selected track index.
    /// </summary>
    public int SelectedTrack { get; }

    /// <summary>
    /// Gets the tracks in the playlist.
    /// </summary>
    public IReadOnlyList<BloomTrack> Tracks { get; }

    internal PlaylistLoadResult(string name, int selectedTrack, IReadOnlyList<BloomTrack> tracks) : base(LoadResultKind.Playlist)
    {
        Name = name;
        SelectedTrack = selectedTrack;
        Tracks = tracks;
    }
}
