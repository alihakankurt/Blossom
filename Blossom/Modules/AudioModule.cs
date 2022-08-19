namespace Blossom.Modules;

[Name("Audio Module")]
public class AudioModule : ModuleBase
{
    private static readonly Dictionary<string, BlossomFilter> filters = new()
    {
        { "reset", new () },
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

    private readonly LavaNode lavaNode;

    private LavaPlayer player;
    private IVoiceState voiceState;

    public AudioModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        lavaNode = serviceProvider.GetService<LavaNode>();

        lavaNode.OnTrackEnded += TrackEnded;
        lavaNode.OnTrackException += TrackException;
        lavaNode.OnTrackStuck += TrackStuck;

        Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
    }

    [Command("join"), Summary("Joins the voice channel")]
    public async Task JoinCommand()
    {
        await JoinAsync(secure: true);
    }

    [Command("leave"), Summary("Leaves the voice channel")]
    public async Task LeaveCommand()
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        await lavaNode.LeaveAsync(player.VoiceChannel);
        await ReplyAsync($"Leaved from {voiceState.VoiceChannel.Mention}");
    }

    [Command("play"), Alias("p"), Summary("Adds the track(s) with given query and plays it")]
    public async Task PlayCommand([Summary("The track's URL or name to search"), Remainder] string query)
    {
        query = query.Trim('<', '>');
        await JoinAsync(secure: false);

        bool secured = await SecureUserStateAsync();
        if (!secured)
        {
            return;
        }

        SearchResponse searchResponse = await lavaNode.SearchAsync(Uri.IsWellFormedUriString(query, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, query);
        if (searchResponse.Status == SearchStatus.LoadFailed || searchResponse.Status == SearchStatus.NoMatches)
        {
            await ReplyAsync("Could not find any tracks with given query!");
            return;
        }

        if (searchResponse.Status == SearchStatus.PlaylistLoaded)
        {
            player.Queue.Enqueue(searchResponse.Tracks);
            await ReplyAsync($"`{searchResponse.Tracks.Count}` tracks queued");
        }
        else
        {
            LavaTrack lavaTrack = searchResponse.Tracks.First();
            player.Queue.Enqueue(lavaTrack);
            await ReplyAsync($"`{lavaTrack.Title}` queued");
        }

        if (player.PlayerState == PlayerState.None || player.PlayerState == PlayerState.Stopped)
        {
            await TryPlayNextAsync(player);
        }
    }

    [Command("stop"), Summary("Stops the current track")]
    public async Task StopCommand()
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (player.Track == null)
        {
            await ReplyAsync("There is no current track to stop!");
        }

        await player.StopAsync();
        await ReplyAsync("Stopped");
    }

    [Command("pause"), Summary("Pauses the current track")]
    public async Task PauseCommand()
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await ReplyAsync("Player is not currently playing!");
            return;
        }

        await player.PauseAsync();
        await ReplyAsync($"`{player.Track.Title}` paused");
    }

    [Command("resume"), Summary("Resumes the current track")]
    public async Task ResumeCommand()
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (player.PlayerState != PlayerState.Paused)
        {
            await ReplyAsync("Player is not currently paused!");
        }

        await player.ResumeAsync();
        await ReplyAsync($"`{player.Track.Title}` resumed");
    }

    [Command("volume"), Alias("vol"), Summary("Sets the playback volume to given value")]
    public async Task VolumeCommand([Summary("The value to set volume")] int volume)
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (volume is not (>= 0 and <= 120))
        {
            await ReplyAsync("The volume must be between 0 and 120!");
            return;
        }

        await player.UpdateVolumeAsync((ushort)volume);
        await ReplyAsync($"Volume setted to {volume}");
    }

    [Command("seek"), Alias("jump"), Summary("Seeks the current track to given timespan (mm:ss)")]
    public async Task ShuffleAsync([Summary("The timespan to seek")] string timespan)
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (player.PlayerState != PlayerState.Playing)
        {
            await ReplyAsync("Player is not currently playing!");
            return;
        }

        Match match = Regex.Match(timespan, "^([0-9]{1,2})[:.]?([0-9]{1,2})?$");
        if (!match.Success)
        {
            await ReplyAsync("Invalid timespan!");
            return;
        }

        TimeSpan position = TimeSpan.FromSeconds(match.Groups[2].Success ? (int.Parse(match.Groups[1].Value) * 60) + int.Parse(match.Groups[2].Value) : int.Parse(match.Groups[1].Value));
        if (player.Track.Duration.CompareTo(position) < 1)
        {
            await ReplyAsync("Timestamp can not exceed the current track's duration!");
            return;
        }

        await player.SeekAsync(position);
        await ReplyAsync($"Seeked to {position:mm':'ss}");
    }

    [Command("filter"), Summary("Applies a filter to audio. Filters:\n`reset`, `bass`, `classical`, `electronic`, `rock`, `soft`, `8d`, `nightcore`, `lovenightcore`, `tremolo`, `vibrato`")]
    public async Task FilterCommand([Summary($"The name of the filter")] string filter)
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (!filters.ContainsKey(filter))
        {
            await ReplyAsync("Invalid name for filter!");
            return;
        }

        BlossomFilter blossomFilter = filters[filter];

        await player.ApplyFiltersAsync(blossomFilter.Filters, 1, blossomFilter.Bands);
        await ReplyAsync($"`{filter}` applied");
    }

    [Command("skip"), Alias("s"), Summary("Skips the current track and plays next if exists")]
    public async Task SkipCommand()
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (player.Queue.Count == 0)
        {
            await player.StopAsync();
            return;
        }

        await TryPlayNextAsync(player);
    }

    [Command("remove"), Alias("rm"), Summary("Removes the track in the given position from queue")]
    public async Task RemoveCommand([Summary("The position of track that will be removed")] int position)
    {
        bool secured = await SecureVoiceStateAsync();
        if (!secured)
        {
            return;
        }

        if (position < 1 || position > player.Queue.Count)
        {
            await ReplyAsync($"The positon must be between 1 and {player.Queue.Count}");
            return;
        }

        LavaTrack removedTrack = player.Queue.RemoveAt(position - 1);
        await ReplyAsync($"`{removedTrack.Title}` dequeued");
    }

    [Command("queue"), Summary("Shows to queue up to 16 tracks")]
    public async Task QueueCommand()
    {
        bool secured = await SecurePlayerAsync();
        if (!secured)
        {
            return;
        }

        await ReplyWithEmbedAsync("Queue", (player.Queue.Count == 0) ? "-- Empty --" : string.Join('\n', player.Queue.Take(16).Select(t => $"[{t.Title} - {t.Author}]({t.Url})")));
    }

    [Command("nowplaying"), Alias("np"), Summary("Shows the currently playing tracks information")]
    public async Task NowPlayingCommand()
    {
        bool secured = await SecurePlayerAsync();
        if (!secured)
        {
            return;
        }

        if (player.Track == null)
        {
            await ReplyAsync("There is no current track to show information!");
            return;
        }

        await ReplyWithEmbedAsync("🎶 Now Playing", $"[{player.Track.Title} - {player.Track.Author}]({player.Track.Url})\n```{(player.Track.IsStream ? "`🔴 LIVE `" : $"\n {"─────────────────────────────".Insert((int)(player.Track.Position.TotalSeconds / player.Track.Duration.TotalSeconds * 30), "⚪")}\n[ {player.Track.Position:hh\\:mm\\:ss} / {player.Track.Duration:hh\\:mm\\:ss} ]")}```", await player.Track.FetchArtworkAsync());
    }

    protected override void BeforeExecute(CommandInfo command)
    {
        lavaNode.TryGetPlayer(Guild, out player);
        voiceState = User as IVoiceState;
    }

    protected override void AfterExecute(CommandInfo command)
    {
        player = null;
        voiceState = null;
    }

    private async Task JoinAsync(bool secure)
    {
        if (secure && player is not null)
        {
            await ReplyAsync("I am already joined to a voice channel!");
            return;
        }

        if (voiceState?.VoiceChannel == null)
        {
            await ReplyAsync("You must be joined to a voice channel!");
            return;
        }

        if (player is null)
        {
            player = await lavaNode.JoinAsync(voiceState.VoiceChannel, Channel as ITextChannel);
            await ReplyAsync($"Joined to {voiceState.VoiceChannel.Mention}");
        }
    }

    private async Task<bool> SecureVoiceStateAsync()
    {
        return await SecurePlayerAsync() && await SecureUserStateAsync();
    }

    private async Task<bool> SecurePlayerAsync()
    {
        if (player is null)
        {
            await ReplyAsync("I am not joined to voice a channel!");
            return false;
        }

        return true;
    }

    private async Task<bool> SecureUserStateAsync()
    {
        if (voiceState?.VoiceChannel != player?.VoiceChannel)
        {
            await ReplyAsync($"You must be connected to {player.VoiceChannel.Mention}!");
            return false;
        }

        return true;
    }

    private static async Task TryPlayNextAsync(LavaPlayer player)
    {
        if (!player.Queue.TryDequeue(out LavaTrack lavaTrack))
        {
            return;
        }

        await player.PlayAsync(lavaTrack);
        await player.TextChannel.SendMessageAsync(embed: new EmbedBuilder()
            .WithTitle("🎶 Now Playing")
            .WithDescription($"[{lavaTrack.Title} - {lavaTrack.Author}]({lavaTrack.Url})")
            .WithColor(Cherry)
            .Build()
        );
    }

    private async Task TrackEnded(TrackEndedEventArgs arg)
    {
        if (arg.Player.PlayerState == PlayerState.Stopped)
        {
            await TryPlayNextAsync(arg.Player);
        }
    }

    private async Task TrackException(TrackExceptionEventArgs arg)
    {
        Logger.Error($"[{arg.Exception.Severity}] (Lavalink) --> {arg.Exception.Message}");
        await TryPlayNextAsync(arg.Player);
    }

    private async Task TrackStuck(TrackStuckEventArgs arg)
    {
        await TryPlayNextAsync(arg.Player);
    }

    private async Task UserVoiceStateUpdated(SocketUser arg, SocketVoiceState before, SocketVoiceState after)
    {
        SocketGuildUser user = arg as SocketGuildUser;
        if (!lavaNode.TryGetPlayer(user.Guild, out LavaPlayer player))
        {
            return;
        }

        if (user.Id == Client.CurrentUser.Id)
        {
            if (before.VoiceChannel != null && after.VoiceChannel == null)
            {
                await lavaNode.LeaveAsync(player.VoiceChannel);
            }
            return;
        }

        IEnumerable<IUser> users = await player.VoiceChannel.GetUsersAsync().FlattenAsync();
        if (!users.Any(u => !u.IsBot && (u as IVoiceState)?.VoiceChannel != null))
        {
            await lavaNode.LeaveAsync(player.VoiceChannel);
        }
    }

    public class EmptyFilter : IFilter
    {
    }

    public struct BlossomFilter
    {
        public List<IFilter> Filters;
        public EqualizerBand[] Bands;

        public BlossomFilter()
        {
            Filters = new List<IFilter> { new EmptyFilter() };
            Bands = new EqualizerBand[15];
            for (int i = 0; i < 15; i++)
            {
                Bands[i] = new EqualizerBand(i, 0);
            }
        }

        public BlossomFilter(params double[] gains) : this()
        {
            for (int i = 0; i < gains.Length; i++)
            {
                Bands[i] = new EqualizerBand(i, gains[i]);
            }
        }

        public BlossomFilter(IFilter filter, params double[] gains) : this(gains)
        {
            Filters[0] = filter;
        }

        public BlossomFilter(List<IFilter> filters, params double[] gains) : this(gains)
        {
            Filters = filters;
        }
    }
}
