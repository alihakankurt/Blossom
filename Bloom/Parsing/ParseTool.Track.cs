using System;
using System.Text.Json.Nodes;

namespace Bloom.Parsing;

internal static partial class ParseTool
{
    internal static BloomTrack ParseTrack(JsonNode node)
    {
        string encoded = node["encoded"]!.GetValue<string>();

        JsonNode info = node["info"]!;

        string identifier = info["identifier"]!.GetValue<string>();
        string title = info["title"]!.GetValue<string>();
        string author = info["author"]!.GetValue<string>();
        string sourceName = info["sourceName"]!.GetValue<string>();

        string? url = info["uri"]?.GetValue<string>();
        string? artworkUrl = info["artworkUri"]?.GetValue<string>();

        bool isSeekable = info["isSeekable"]!.GetValue<bool>();
        bool isStream = info["isStream"]!.GetValue<bool>();

        TimeSpan duration = TimeSpan.Zero;
        TimeSpan position = TimeSpan.Zero;
        if (!isStream)
        {
            duration = TimeSpan.FromMilliseconds(info["length"]!.GetValue<long>());
            position = TimeSpan.FromMilliseconds(info["position"]!.GetValue<long>());
        }

        return new BloomTrack(encoded, identifier, title, author, sourceName, url, artworkUrl, isSeekable, isStream, duration, position);
    }
}
