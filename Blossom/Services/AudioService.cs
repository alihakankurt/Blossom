namespace Blossom.Services;

public static class AudioService
{
    public static void Start(BloomNode bloomNode)
    {
        bloomNode.NodeException += NodeException;
        bloomNode.TrackStarted += TrackStarted;
        bloomNode.TrackEnded += TrackEnded;
        bloomNode.TrackException += TrackException;
        bloomNode.TrackStucked += TrackStucked;
    }

    public static FilterPreset? GetFilterPreset(string name)
    {
        return FilterPreset.Presets.FirstOrDefault(
            (preset) => string.CompareOrdinal(preset.Name, 0, name, 0, name.Length) is 0
        );
    }

    private static Task NodeException(NodeExceptionEventArgs args)
    {
        Console.WriteLine($"LavalinkException: {args.Message}");
        return Task.CompletedTask;
    }

    private static async Task TrackStarted(TrackStartEventArgs args)
    {
        await args.Player.TextChannel.SendEmbedAsync(
            title: "🎶 Now Playing",
            description: $"[{args.Player.Track!.Title}]({args.Player.Track.Url})\n",
            color: BaseInteractionModule.Cherry
        );
    }

    private static Task TrackEnded(TrackEndEventArgs args)
    {
        if (args.ShouldPlayNext && (args.Player.Queue.HasNext || args.Player.Queue.LoopMode is not LoopMode.None))
            return args.Player.PlayNextAsync();

        return args.Player.StopAsync();
    }

    private static Task TrackException(TrackExceptionEventArgs args)
    {
        if (args.Player.Queue.HasNext)
            return args.Player.PlayNextAsync();

        return args.Player.StopAsync();
    }

    private static Task TrackStucked(TrackStuckEventArgs args)
    {
        if (args.Player.Queue.HasNext)
            return args.Player.PlayNextAsync();

        return args.Player.StopAsync();
    }
}
