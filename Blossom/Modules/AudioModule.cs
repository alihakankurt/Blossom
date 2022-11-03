namespace Blossom.Modules;

[EnabledInDm(false)]
public sealed class AudioModule : BaseInteractionModule
{
    private const string StreamDescription = "`🔴 LIVE `";
    private const string PositionBar = "─────────────────────────────";
    private const string PositionSign = "⚪";

    private readonly AudioService _audioService;

    public AudioModule(IServiceProvider services, AudioService audioService) : base(services)
    {
        _audioService = audioService;
    }

    [SlashCommand("join", "Joins the voice channel"), RequirePlayerNotJoined, RequireVoiceChannel]
    public async Task JoinCommand()
    {
        LavaPlayer player = await _audioService.JoinAsync(((IVoiceState)User).VoiceChannel, (ITextChannel)Channel);
        await RespondAsync($"Joined to {player.VoiceChannel.Mention}");
    }

    [SlashCommand("leave", "Leaves the voice channel"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task LeaveCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        IVoiceChannel voiceChannel = player.VoiceChannel;
        await _audioService.LeaveAsync(voiceChannel);
        await RespondAsync($"Leaved from {voiceChannel.Mention}");
    }

    [SlashCommand("play", "Adds the track(s) with given query"), RequireUserInVoiceChannel, RequireVoiceChannel]
    public async Task PlayCommand([Summary(description: "The track's URL or name to search")] string query)
    {
        LavaPlayer player = _audioService.GetPlayer(Guild) ?? await _audioService.JoinAsync(((IVoiceState)User).VoiceChannel, (ITextChannel)Channel);

        SearchResponse searchResponse = await _audioService.SearchAsync(query);
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
        {
            await RespondEphemeralAsync("Could't find any tracks with given `query`!");
            return;
        }

        if (searchResponse.Status is SearchStatus.PlaylistLoaded)
        {
            player.Vueue.Enqueue(searchResponse.Tracks);
            await RespondAsync($"{searchResponse.Playlist.Name}` loaded with `{searchResponse.Tracks.Count}` tracks.");
        }
        else
        {
            LavaTrack firstTrack = searchResponse.Tracks.First();
            player.Vueue.Enqueue(firstTrack);
            await RespondAsync($"`{firstTrack.Title.CutOff(70)}` queued.");
        }

        if (player.PlayerState is PlayerState.None or PlayerState.Stopped)
        {
            _ = player.Vueue.TryDequeue(out LavaTrack? track);
            await player.PlayAsync(track);
        }
    }

    [SlashCommand("stop", "Stops the player and clears the queue"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task StopCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await _audioService.StopAsync(player);
        await RespondAsync("Player stopped.");
    }

    [SlashCommand("pause", "Pauses the player"), RequirePlayerJoined, RequireUserInVoiceChannel, EnsureVoiceState(PlayerState.Playing)]
    public async Task PauseCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await player.PauseAsync();
        await RespondAsync("Player paused.");
    }

    [SlashCommand("resume", "Resumes the player"), RequirePlayerJoined, RequireUserInVoiceChannel, EnsureVoiceState(PlayerState.Paused)]
    public async Task ResumeCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await player.ResumeAsync();
        await RespondAsync("Player resumed.");
    }

    [SlashCommand("volume", "Sets the player's volume"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task VolumeCommand([Summary(description: "The volume value"), MinValue(0), MaxValue(120)] ushort volume)
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await player.SetVolumeAsync(volume);
        await RespondAsync($"Volume setted to `{volume}`.");
    }

    [SlashCommand("seek", "Seeks the current track to given timespan (mm:ss)"), RequirePlayerJoined, RequireUserInVoiceChannel, EnsureVoiceState(PlayerState.Playing)]
    public async Task SeekAsync([Summary(description: "The timespan to seek")] TimeSpan timespan)
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;

        if (player.Track.Duration.CompareTo(timespan) < 1)
        {
            await RespondAsync("The `timespan` can't exceed the current track's duration!");
            return;
        }

        await player.SeekAsync(timespan);
        await RespondAsync($"Seeked to `{timespan:mm':'ss}`.");
    }

    [SlashCommand("filter", "Applies a filter to audio"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task FilterCommand([Summary(description: "The name of the filter"), Autocomplete(typeof(FilterAutocompleteHandler))] string filter)
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await _audioService.ApplyFilterAsync(player, filter);
        await RespondAsync("Filter applied.");
    }

    [SlashCommand("skip", "Stops the current track and plays next one if exists"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task SkipCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await _audioService.SkipAsync(player);
        await RespondAsync("Skipped.");
    }

    [SlashCommand("remove", "Removes the track in the given position from queue"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task RemoveCommand([Summary(description: "The position of track that will be removed"), MinValue(1), Autocomplete(typeof(RemoveTrackAutocompleteHandler))] int position)
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        if (position > player.Vueue.Count)
        {
            await RespondAsync($"The `positon` must be lesser than `{player.Vueue.Count}`!");
            return;
        }

        LavaTrack removedTrack = player.Vueue.RemoveAt(position - 1);
        await RespondAsync($"`{removedTrack.Title.CutOff(70)}` dequeued.");
    }

    [SlashCommand("queue", "Shows to queue up to 16 tracks"), RequirePlayerJoined]
    public async Task QueueCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        await RespondWithEmbedAsync(
            title: $"Track Queue - {player.Vueue.Count} tracks",
            description: string.Join('\n', player.Vueue.Take(16)
                    .Select(static (track) => $"[{track.Title} - {track.Author}]({track.Url})"))
        );
    }

    [SlashCommand("nowplaying", "Shows the currently playing track's information"), RequirePlayerJoined]
    public async Task NowPlayingCommand()
    {
        LavaPlayer player = _audioService.GetPlayer(Guild)!;
        if (player.Track is null)
        {
            await RespondEphemeralAsync("Nothing is playing right now!");
            return;
        }

        string description = $"[{player.Track.Title} - {player.Track.Author}]({player.Track.Url})\n"
            + $"```\n{(player.Track.IsStream ? StreamDescription : PositionBar.Insert((int)(player.Track.Position.TotalSeconds / player.Track.Duration.TotalSeconds * 30), PositionSign))}\n\n"
            + $"[ {player.Track.Position:hh\\:mm\\:ss} / {player.Track.Duration:hh\\:mm\\:ss} ]```";

        string artwork = await player.Track.FetchArtworkAsync();

        await RespondWithEmbedAsync(
            title: "🎶 Now Playing",
            description: description,
            thumbnail: artwork
        );
    }
}
