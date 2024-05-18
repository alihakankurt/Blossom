using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
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
            BloomException exception = ParseException(node["data"]!);
            return new ErrorLoadResult(exception);
        }
        else if (loadType == "search")
        {
            IReadOnlyList<BloomTrack> tracks = ParseTrackArray(node["data"]!);
            return new SearchLoadResult(tracks);
        }
        else if (loadType == "track")
        {
            BloomTrack track = ParseTrack(node["data"]!);
            return new TrackLoadResult(track);
        }
        else if (loadType == "playlist")
        {
            JsonNode info = node["data"]!["info"]!;
            string name = info["name"]!.GetValue<string>();
            int selectedTrack = info["selectedTrack"]!.GetValue<int>();
            IReadOnlyList<BloomTrack> tracks = ParseTrackArray(node["data"]!["tracks"]!);
            return new PlaylistLoadResult(name, selectedTrack, tracks);
        }
        else
        {
            throw new InvalidOperationException($"Unknown load type: {loadType}");
        }
    }

    private static IReadOnlyList<BloomTrack> ParseTrackArray(JsonNode node)
    {
        return [.. node.AsArray().Select(static (node) => ParseTrack(node!))];
    }
}
