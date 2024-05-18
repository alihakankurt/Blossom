using System;

namespace Bloom;

/// <summary>
/// Represents a track that can be played by a <see cref="BloomPlayer"/>.
/// </summary>
public sealed class BloomTrack : IEquatable<BloomTrack>
{
    /// <summary>
    /// The base64-encoded track data.
    /// </summary>
    public string Encoded { get; }

    /// <summary>
    /// The unique identifier of the track.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// The title of the track.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The author of the track.
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// The name of the source of the track.
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// The URL of the track.
    /// </summary>
    public string? Url { get; }

    /// <summary>
    /// The URL of the artwork of the track.
    /// </summary>
    public string? ArtworkUrl { get; }

    /// <summary>
    /// Gets a value indicating whether the track is seekable.
    /// </summary>
    public bool IsSeekable { get; }

    /// <summary>
    /// Gets a value indicating whether the track is a stream.
    /// </summary>
    public bool IsStream { get; }

    /// <summary>
    /// The duration of the track. It is <see cref="TimeSpan.Zero"/> if the track is a stream.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// The position of the track. It is <see cref="TimeSpan.Zero"/> if the track is a stream.
    /// </summary>
    public TimeSpan Position { get; internal set; }

    internal BloomTrack(string encoded, string identifier, string title, string author, string sourceName, string? url, string? artworkUrl, bool isSeekable, bool isStream, TimeSpan duration, TimeSpan position)
    {
        Encoded = encoded;
        Identifier = identifier;
        Title = title;
        Author = author;
        SourceName = sourceName;
        Url = url;
        ArtworkUrl = artworkUrl;
        IsSeekable = isSeekable;
        IsStream = isStream;
        Duration = duration;
        Position = position;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BloomTrack"/> is equal to the current <see cref="BloomTrack"/> based on the <see cref="Identifier"/>.
    /// </summary>
    /// <param name="other">The other <see cref="BloomTrack"/> to compare.</param>
    /// <returns>Whether the tracks are equal.</returns>
    public bool Equals(BloomTrack? other)
    {
        return other is not null && Identifier == other.Identifier;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="BloomTrack"/>.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare.</param>
    /// <returns>Whether the objects are equal.</returns>
    public override bool Equals(object? obj)
    {
        return obj is BloomTrack track && Equals(track);
    }

    /// <summary>
    /// Returns the hash code for the <see cref="BloomTrack"/>.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return Identifier.GetHashCode();
    }

    public static bool operator ==(BloomTrack? left, BloomTrack? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(BloomTrack? left, BloomTrack? right)
    {
        return !(left == right);
    }
}
