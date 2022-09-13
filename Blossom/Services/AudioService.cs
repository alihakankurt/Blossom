namespace Blossom.Services;

public static class AudioService
{
    public static readonly Dictionary<string, AudioFilter> Filters = new()
    {
        { "flat", new() },
        { "bass", new (0.6, 0.7, 0.8, 0.55, 0.25, 0, -0.25, -0.25, -0.25, -0.25, -0.25, -0.25) },
        { "classical", new (0.375, 0.350, 0.125, 0, 0, 0.125, 0.550, 0.050, 0.125, 0.250, 0.200, 0.250, 0.300, 0.250, 0.300) },
        { "electronic", new(0.375, 0.350, 0.125, 0, 0, -0.125, -0.125, 0, 0.25, 0.125, 0.15, 0.2, 0.250, 0.350, 0.400) },
        { "rock", new(0.300, 0.250, 0.200, 0.100, 0.050, -0.050, -0.150, -0.200, -0.100, -0.050, 0.050, 0.100, 0.200, 0.250, 0.300) },
        { "soft", new(new LowPassFilter { Smoothing = 20 }) },
        { "8d", new(new RotationFilter { Hertz = 0.2 }) },
        { "nightcore", new(new TimescaleFilter { Speed = 1.3, Pitch = 1.3, Rate = 1.0 }) },
        { "lovenightcore", new(new TimescaleFilter { Speed = 1.1, Pitch = 1.2, Rate = 1.0 }) },
        { "tremolo", new(new TremoloFilter { Frequency = 10, Depth = 0.5 }) },
        { "vibrato", new(new VibratoFilter { Frequency = 10, Depth = 0.9 }) },
    };

    private static LavaNode _lavaNode;
    private static DiscordSocketClient _client;

    public static void Initialize(IServiceProvider services)
    {
        _lavaNode = services.GetService<LavaNode>();
        _client = services.GetService<DiscordSocketClient>();

        _lavaNode.ConnectAsync();
        _lavaNode.OnTrackEnded += OnTrackEnded;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnTrackException += OnTrackException;
        _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
    }

    public static LavaPlayer GetPlayer(IGuild guild)
    {
        if (_lavaNode.TryGetPlayer(guild, out LavaPlayer player))
        {
            return player;
        }

        return null;
    }

    public static async Task<SearchResponse> SearchAsync(string query)
    {
        query = query.Trim('<', '>');
        bool isWellFormatted = Uri.IsWellFormedUriString(query, UriKind.Absolute);
        SearchResponse searchResponse = await _lavaNode.SearchAsync(isWellFormatted ? SearchType.Direct : SearchType.YouTube, query);
        return searchResponse;
    }

    public static async Task<LavaPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel)
    {
        return await _lavaNode.JoinAsync(voiceChannel, textChannel);
    }

    public static async Task LeaveAsync(IVoiceChannel voiceChannel)
    {
        await _lavaNode.LeaveAsync(voiceChannel);
    }

    public static async Task StopAsync(LavaPlayer player)
    {
        player.Queue.Clear();
        await player.StopAsync();
    }

    public static async Task SkipAsync(LavaPlayer player)
    {
        await player.StopAsync();
    }

    public static async Task ApplyFilterAsync(LavaPlayer player, string filter)
    {
        if (!Filters.TryGetValue(filter, out AudioFilter audioFilter))
        {
            audioFilter = Filters["empty"];
        }

        await player.ApplyFiltersAsync(audioFilter.Filters, 1, audioFilter.Bands);
    }

    public static async Task TryPlayNextAsync(LavaPlayer player)
    {
        if (player.PlayerState != PlayerState.None && player.PlayerState != PlayerState.Stopped)
        {
            return;
        }

        if (player.IsConnected && player.Queue.TryDequeue(out LavaTrack lavaTrack))
        {
            await player.PlayAsync(lavaTrack);
        }
    }

    private static async Task OnTrackEnded(TrackEndedEventArgs arg)
    {
        await TryPlayNextAsync(arg.Player);
    }

    private static async Task OnTrackException(TrackExceptionEventArgs arg)
    {
        LoggerService.Error($"[{arg.Exception.Severity}] (Lavalink) --> {arg.Exception.Message}");
        await TryPlayNextAsync(arg.Player);
    }

    private static async Task OnTrackStuck(TrackStuckEventArgs arg)
    {
        await TryPlayNextAsync(arg.Player);
    }

    private static async Task OnUserVoiceStateUpdated(SocketUser arg, SocketVoiceState before, SocketVoiceState after)
    {
        if (!_lavaNode.TryGetPlayer((arg as SocketGuildUser).Guild, out LavaPlayer player))
        {
            return;
        }

        if (arg.Id == _client.CurrentUser.Id)
        {
            if (before.VoiceChannel != null && after.VoiceChannel == null)
            {
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }

            return;
        }

        IEnumerable<IUser> users = await player.VoiceChannel.GetUsersAsync().FlattenAsync();
        if (!users.Any((user) => !user.IsBot && (user as IVoiceState)?.VoiceChannel != null))
        {
            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }
    }
}
