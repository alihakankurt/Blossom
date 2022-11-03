namespace Blossom.Services;

public sealed class AudioService
{
    public static readonly Dictionary<string, Filter> Filters;

    private readonly LavaNode<LavaPlayer, LavaTrack> _lavaNode;
    private readonly DiscordSocketClient _discordClient;

    static AudioService()
    {
        Filters = new Dictionary<string, Filter>()
        {
            { "flat", Filter.Flat },
            { "bass", new Filter(0.6, 0.7, 0.8, 0.55, 0.25, 0, -0.25, -0.25, -0.25, -0.25, -0.25, -0.25) },
            { "classical", new Filter(0.375, 0.350, 0.125, 0, 0, 0.125, 0.550, 0.050, 0.125, 0.250, 0.200, 0.250, 0.300, 0.250, 0.300) },
            { "electronic", new Filter(0.375, 0.350, 0.125, 0, 0, -0.125, -0.125, 0, 0.25, 0.125, 0.15, 0.2, 0.250, 0.350, 0.400) },
            { "rock", new Filter(0.300, 0.250, 0.200, 0.100, 0.050, -0.050, -0.150, -0.200, -0.100, -0.050, 0.050, 0.100, 0.200, 0.250, 0.300) },
            { "soft", new Filter(new LowPassFilter { Smoothing = 20 }) },
            { "8d", new Filter(new RotationFilter { Hertz = 0.2 }) },
            { "nightcore", new Filter(new TimescaleFilter { Speed = 1.3, Pitch = 1.3, Rate = 1.0 }) },
            { "lovenightcore", new(new TimescaleFilter { Speed = 1.1, Pitch = 1.2, Rate = 1.0 }) },
            { "tremolo", new Filter(new TremoloFilter { Frequency = 10, Depth = 0.5 }) },
            { "vibrato", new Filter(new VibratoFilter { Frequency = 10, Depth = 0.9 }) },
        };
    }

    public AudioService(LavaNode<LavaPlayer, LavaTrack> lavaNode, DiscordSocketClient discordClient)
    {
        _lavaNode = lavaNode;
        _discordClient = discordClient;

        _lavaNode.OnTrackEnd += static (e) => PlayNextAsync(e.Player);
        _lavaNode.OnTrackStuck += static (e) => PlayNextAsync(e.Player);
        _lavaNode.OnTrackException += static (e) => PlayNextAsync(e.Player);
        _discordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;
    }

    public Task ConnectAsync()
    {
        return _lavaNode.ConnectAsync();
    }

    public bool IsJoined(IGuild guild)
    {
        return _lavaNode.HasPlayer(guild);
    }

    public LavaPlayer? GetPlayer(IGuild guild)
    {
        _ = _lavaNode.TryGetPlayer(guild, out LavaPlayer? player);
        return player;
    }

    public Task<SearchResponse> SearchAsync(string query)
    {
        query = query.Trim('<', '>');
        bool isWellFormatted = Uri.IsWellFormedUriString(query, UriKind.Absolute);
        return _lavaNode.SearchAsync(isWellFormatted ? SearchType.Direct : SearchType.YouTube, query);
    }

    public Task<LavaPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel)
    {
        return _lavaNode.JoinAsync(voiceChannel, textChannel);
    }

    public Task LeaveAsync(IVoiceChannel voiceChannel)
    {
        return _lavaNode.LeaveAsync(voiceChannel);
    }

    public Task StopAsync(LavaPlayer player)
    {
        player.Vueue.Clear();
        return player.StopAsync();
    }

    public Task SkipAsync(LavaPlayer player)
    {
        return player.StopAsync();
    }

    public Task ApplyFilterAsync(LavaPlayer player, string filter)
    {
        if (!Filters.TryGetValue(filter, out Filter? audioFilter))
        {
            audioFilter = Filters["empty"];
        }

        return player.ApplyFiltersAsync(audioFilter.Filters, volume: 1, audioFilter.Bands);
    }

    public static async Task PlayNextAsync(LavaPlayer player)
    {
        if (player.Vueue.TryDequeue(out LavaTrack? track))
        {
            await player.PlayAsync(track);
        }
    }

    private async Task UserVoiceStateUpdated(SocketUser arg, SocketVoiceState before, SocketVoiceState after)
    {
        LavaPlayer? player = GetPlayer(((SocketGuildUser)arg).Guild);
        if (player is null)
        {
            return;
        }

        if (arg.Id == _discordClient.CurrentUser.Id)
        {
            if (before.VoiceChannel is not null && after.VoiceChannel is null)
            {
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }

            return;
        }

        IEnumerable<IUser> users = await player.VoiceChannel.GetUsersAsync().FlattenAsync();
        if (users.Any(static (user) => !user.IsBot && ((IVoiceState)user).VoiceChannel is not null))
        {
            return;
        }

        await _lavaNode.LeaveAsync(player.VoiceChannel);
    }
}

public class Filter
{
    public static readonly Filter Flat;

    public readonly IList<IFilter> Filters;
    public readonly EqualizerBand[] Bands;

    static Filter()
    {
        Flat = new Filter();
    }

    public Filter(params double[] gains)
    {
        Filters = new List<IFilter>();
        Bands = Enumerable.Range(0, 15)
            .Select((i) => new EqualizerBand(i, (i < gains.Length) ? gains[i] : 0))
            .ToArray();
    }

    public Filter(IFilter filter, params double[] gains) : this(gains)
    {
        Filters.Add(filter);
    }

    public Filter(List<IFilter> filters, params double[] gains) : this(gains)
    {
        Filters = filters;
    }
}
