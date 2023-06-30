namespace Bloom;

public sealed class BloomTrack
{
    public string Encoded { get; init; }
    public string Identifier { get; init; }
    public string Title { get; init; }
    public string Author { get; init; }
    public string SourceName { get; init; }
    public string? Url { get; init; }
    public bool IsSeekable { get; init; }
    public bool IsStream { get; init; }
    public TimeSpan Duration { get; init; }
    public TimeSpan Position { get; private set; }

    private BloomTrack(string encoded, string identifier, string title, string author, string sourceName, string? url, bool isSeekable, bool isStream, TimeSpan duration, TimeSpan position)
    {
        Encoded = encoded;
        Identifier = identifier;
        Title = title;
        Author = author;
        SourceName = sourceName;
        Url = url;
        IsSeekable = isSeekable;
        IsStream = isStream;
        Duration = duration;
        Position = position;
    }

    internal static BloomTrack Parse(JsonNode data)
    {
        string encoded = data["encoded"]!.ToString();
        JsonNode info = data["info"]!;
        string identifier = info["identifier"]!.ToString();
        string title = info["title"]!.ToString();
        string author = info["author"]!.ToString();
        string sourceName = info["sourceName"]!.ToString();
        string? url = info["uri"]!.ToString();
        bool isSeekable = bool.Parse(info["isSeekable"]!.ToString());
        bool isStream = bool.Parse(info["isStream"]!.ToString());
        TimeSpan duration = TimeSpan.Zero;
        TimeSpan position = TimeSpan.Zero;
        if (!isStream)
        {
            duration = TimeSpan.FromMilliseconds(long.Parse(info["length"]!.ToString()));
            position = TimeSpan.FromMilliseconds(long.Parse(info["position"]!.ToString()));
        }

        return new BloomTrack(encoded, identifier, title, author, sourceName, url, isSeekable, isStream, duration, position);
    }

    internal void UpdatePosition(long position)
    {
        Position = TimeSpan.FromMilliseconds(position);
    }
}
