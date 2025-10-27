using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Bloom.Playback;
using Bloom.Searching;

namespace Bloom.Parsing;

internal static partial class ParseTool
{
    public static LoadResult ParseLoadResult(JsonNode? node)
    {
        if (node is null)
            return new EmptyLoadResult();

        string loadType = node["loadType"]!.GetValue<string>();

        if (loadType == "empty")
        {
            return new EmptyLoadResult();
        }
        else if (loadType == "error")
        {
            BloomException exception = ParseTool.ParseException(node["data"]!);
            return new ErrorLoadResult(exception);
        }
        else if (loadType == "search")
        {
            ImmutableArray<BloomTrack> tracks = ParseTool.ParseTrackArray(node["data"]!);
            return new SearchLoadResult(tracks);
        }
        else if (loadType == "track")
        {
            BloomTrack track = ParseTool.ParseTrack(node["data"]!);
            return new TrackLoadResult(track);
        }
        else if (loadType == "playlist")
        {
            JsonNode info = node["data"]!["info"]!;
            string name = info["name"]!.GetValue<string>();
            int selectedTrack = info["selectedTrack"]!.GetValue<int>();
            ImmutableArray<BloomTrack> tracks = ParseTool.ParseTrackArray(node["data"]!["tracks"]!);
            return new PlaylistLoadResult(name, selectedTrack, tracks);
        }
        else
        {
            throw new InvalidOperationException($"Unknown load type: {loadType}");
        }
    }

    private static ImmutableArray<BloomTrack> ParseTrackArray(JsonNode node)
    {
        return node.AsArray().Select(static (node) => ParseTool.ParseTrack(node!)).ToImmutableArray();
    }
}
