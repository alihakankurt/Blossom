namespace Bloom;

public sealed class SearchResult
{
    public SearchResultKind Kind { get; }
    public IReadOnlyList<BloomTrack>? Tracks { get; }
    public PlaylistInfo? Playlist { get; }
    public string? ExceptionMessage { get; }

    private SearchResult(SearchResultKind kind)
    {
        Kind = kind;
    }

    private SearchResult(SearchResultKind kind, IReadOnlyList<BloomTrack> tracks) : this(kind)
    {
        Tracks = tracks;
    }

    private SearchResult(SearchResultKind kind, IReadOnlyList<BloomTrack> tracks, PlaylistInfo playlist) : this(kind, tracks)
    {
        Playlist = playlist;
    }

    private SearchResult(SearchResultKind kind, string exceptionMessage) : this(kind)
    {
        ExceptionMessage = exceptionMessage;
    }

    internal static SearchResult Parse(JsonNode data)
    {
        return data["loadType"]!.ToString() switch
        {
            "LOAD_FAILED" => new SearchResult(
                SearchResultKind.LoadFailed,
                data["exception"]!["message"]!.ToString()
            ),
            "SEARCH_RESULT" => new SearchResult(
                SearchResultKind.SearchResult,
                (data["tracks"] as JsonArray)!
                    .Select(static (t) => BloomTrack.Parse(t!))
                    .ToList()
            ),
            "TRACK_LOADED" => new SearchResult(
                SearchResultKind.TrackLoaded,
                (data["tracks"] as JsonArray)!
                    .Select(static (t) => BloomTrack.Parse(t!))
                    .ToList()
            ),
            "PLAYLIST_LOADED" => new SearchResult(
                SearchResultKind.PlaylistLoaded,
                (data["tracks"] as JsonArray)!
                    .Select(static (t) => BloomTrack.Parse(t!))
                    .ToList(),
                PlaylistInfo.Parse(data["playlistInfo"]!)
            ),
            _ => new SearchResult(
                SearchResultKind.NoMatches
            ),
        };
    }

    public sealed class PlaylistInfo
    {
        public string Name { get; }
        public int Selected { get; }

        private PlaylistInfo(string name, int selected)
        {
            Name = name;
            Selected = selected;
        }

        internal static PlaylistInfo Parse(JsonNode info)
        {
            return new PlaylistInfo(
                info["name"]!.ToString(),
                int.Parse(info["selectedTrack"]!.ToString())
            );
        }
    }
}
