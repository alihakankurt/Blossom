using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bloom;
using Bloom.Events;
using Bloom.Filters;
using Bloom.Playback;
using Bloom.Searching;
using Blossom.Modules;
using Blossom.Utilities;
using Discord;

namespace Blossom.Services;

public sealed class AudioService : IService
{
    private readonly BloomNode _node;
    private readonly SomeRandomApi _someRandomApi;

    public AudioService(BloomNode node, SomeRandomApi someRandomApi)
    {
        _node = node;
        _someRandomApi = someRandomApi;
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        _node.NodeException += NodeException;
        _node.TrackStarted += TrackStarted;
        _node.TrackEnded += TrackEnded;
        _node.TrackException += TrackException;
        _node.TrackStucked += TrackStucked;

        await _node.ConnectAsync();
    }

    public bool HasPlayer(IGuild guild)
    {
        return _node.TryGetPlayer(guild, out _);
    }

    public BloomPlayer? GetPlayer(IGuild guild)
    {
        _ = _node.TryGetPlayer(guild, out BloomPlayer? player);
        return player;
    }

    public async ValueTask<BloomPlayer> JoinAsync(IVoiceChannel voiceChannel, IMessageChannel textChannel)
    {
        return await _node.JoinAsync(voiceChannel, textChannel);
    }

    public async ValueTask<BloomPlayer> GetOrJoinAsync(IVoiceChannel voiceChannel, IMessageChannel textChannel)
    {
        if (_node.TryGetPlayer(voiceChannel.Guild, out BloomPlayer? player))
            return player;

        return await JoinAsync(voiceChannel, textChannel);
    }

    public async ValueTask LeaveAsync(IGuild guild)
    {
        await _node.LeaveAsync(guild);
    }

    public async ValueTask<LoadResult> SearchAsync(string query)
    {
        query = query.Trim('<', '>');
        bool isWellFormed = Uri.IsWellFormedUriString(query, UriKind.Absolute);
        SearchKind searchKind = isWellFormed ? SearchKind.Direct : SearchKind.YouTube;
        LoadResult result = await _node.SearchAsync(query, searchKind);
        return result;
    }

    public async ValueTask<LyricsResult?> GetLyricsAsync(BloomTrack bloomTrack)
    {
        return await _someRandomApi.GetLyricsAsync(bloomTrack.Title);
    }

    private static Task NodeException(NodeExceptionEventArgs args)
    {
        Console.WriteLine($"{nameof(BloomNode)}: {args.Message}");
        return Task.CompletedTask;
    }

    public FilterPreset? GetFilterPreset(string name)
    {
        return FilterPreset.Presets.FirstOrDefault(
            (preset) => string.Compare(preset.Name, name, StringComparison.OrdinalIgnoreCase) == 0
        );
    }

    private static async Task TrackStarted(TrackStartEventArgs args)
    {
        Embed embed = EmbedUtility.CreateEmbed(
            title: "🎶 Now Playing",
            description: $"[{args.Player.Track!.Title}]({args.Player.Track.Url})\n",
            color: BaseInteractionModule.Cherry
        );

        await args.Player.TextChannel.SendMessageAsync(embed: embed);
    }

    private static async Task TrackEnded(TrackEndEventArgs args)
    {
        Console.WriteLine(args.EndReason);
        if (args.MayStartNext && args.Player.Queue.HasNext)
        {
            await args.Player.PlayNextAsync();
            return;
        }

        await args.Player.StopAsync();
    }

    private static async Task TrackException(TrackExceptionEventArgs args)
    {
        if (args.Player.Queue.HasNext)
            await args.Player.PlayNextAsync();

        await args.Player.StopAsync();
    }

    private static async Task TrackStucked(TrackStuckEventArgs args)
    {
        if (args.Player.Queue.HasNext)
            await args.Player.PlayNextAsync();

        await args.Player.StopAsync();
    }
}
