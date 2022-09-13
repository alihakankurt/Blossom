namespace Blossom.Modules;

[EnabledInDm(isEnabled: false)]
public sealed class AudioModule : InteractionModuleBase
{
    private LavaPlayer _player;

    public AudioModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override void BeforeExecute(ICommandInfo command)
    {
        _player = AudioService.GetPlayer(Guild);
    }

    [SlashCommand("join", "Joins the voice channel"), RequirePlayerNotJoined, RequireVoiceChannel]
    public async Task JoinCommand()
    {
        _player = await AudioService.JoinAsync((User as IVoiceState).VoiceChannel, Channel as ITextChannel);
        await RespondAsync($"Joined to {_player.VoiceChannel.Mention}");
    }

    [SlashCommand("leave", "Leaves the voice channel"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task LeaveCommand()
    {
        IVoiceChannel voiceChannel = _player.VoiceChannel;
        await AudioService.LeaveAsync(voiceChannel);
        await RespondAsync($"Leaved from {voiceChannel.Mention}");
    }

    [SlashCommand("play", "Adds the track(s) with given query"), RequireUserInVoiceChannel, RequireVoiceChannel]
    public async Task PlayCommand([Summary(description: "The track's URL or name to search")] string query)
    {
        _player ??= await AudioService.JoinAsync((User as IVoiceState).VoiceChannel, Channel as ITextChannel);

        SearchResponse searchResponse = await AudioService.SearchAsync(query);
        if (searchResponse.Status == SearchStatus.LoadFailed || searchResponse.Status == SearchStatus.NoMatches)
        {
            await RespondEphemeralAsync("Could't find any tracks with given `query`!");
            return;
        }

        if (searchResponse.Status == SearchStatus.PlaylistLoaded)
        {
            _player.Queue.Enqueue(searchResponse.Tracks);
            await RespondAsync($"`{searchResponse.Tracks.Count}` tracks queued.");
        }
        else
        {
            LavaTrack firstTrack = searchResponse.Tracks.First();
            _player.Queue.Enqueue(firstTrack);
            await RespondAsync($"`{CutOff(firstTrack.Title, 70)}` queued.");
        }

        await AudioService.TryPlayNextAsync(_player);
    }

    [SlashCommand("stop", "Stops the player and clears the queue"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task StopCommand()
    {
        await AudioService.StopAsync(_player);
        await RespondAsync("Player stopped.");
    }

    [SlashCommand("pause", "Pauses the player"), RequirePlayerJoined, RequireUserInVoiceChannel, EnsureVoiceState(PlayerState.Playing)]
    public async Task PauseCommand()
    {
        await _player.PauseAsync();
        await RespondAsync("Player paused.");
    }

    [SlashCommand("resume", "Resumes the player"), RequirePlayerJoined, RequireUserInVoiceChannel, EnsureVoiceState(PlayerState.Paused)]
    public async Task ResumeCommand()
    {
        await _player.ResumeAsync();
        await RespondAsync("Player resumed.");
    }

    [SlashCommand("volume", "Sets the player's volume"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task VolumeCommand([Summary(description: "The volume value"), MinValue(0), MaxValue(120)] ushort volume)
    {
        await _player.UpdateVolumeAsync(volume);
        await RespondAsync($"Volume setted to `{volume}`.");
    }

    [SlashCommand("seek", "Seeks the current track to given timespan (mm:ss)"), RequirePlayerJoined, RequireUserInVoiceChannel, EnsureVoiceState(PlayerState.Playing)]
    public async Task SeekAsync([Summary(description: "The timespan to seek")] string timespan)
    {
        Match match = Regex.Match(timespan, "^([0-9]{1,2})[:.]?([0-9]{1,2})?$");
        if (!match.Success)
        {
            await RespondEphemeralAsync("Invalid `timespan`!");
            return;
        }

        TimeSpan position = TimeSpan.FromSeconds(match.Groups[2].Success ? (int.Parse(match.Groups[1].Value) * 60) + int.Parse(match.Groups[2].Value) : int.Parse(match.Groups[1].Value));
        if (_player.Track.Duration.CompareTo(position) < 1)
        {
            await RespondAsync("The `timespan` can't exceed the current track's duration!");
            return;
        }

        await _player.SeekAsync(position);
        await RespondAsync($"Seeked to `{position:mm':'ss}`.");
    }

    [SlashCommand("filter", "Applies a filter to audio"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task FilterCommand([Summary(description: "The name of the filter"), Autocomplete(typeof(FilterAutocompleteHandler))] string filter)
    {
        await AudioService.ApplyFilterAsync(_player, filter);
        await RespondAsync("Filter applied.");
    }

    [SlashCommand("skip", "Stops the current track and plays next one if exists"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task SkipCommand()
    {
        await AudioService.SkipAsync(_player);
        await RespondAsync("Skipped.");
    }

    [SlashCommand("remove", "Removes the track in the given position from queue"), RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task RemoveCommand([Summary(description: "The position of track that will be removed"), MinValue(1), Autocomplete(typeof(TrackAutocompleteHandler))] int position)
    {
        if (position > _player.Queue.Count)
        {
            await RespondAsync($"The `positon` must be lesser than `{_player.Queue.Count}`!");
            return;
        }

        LavaTrack removedTrack = _player.Queue.RemoveAt(position - 1);
        await RespondAsync($"`{CutOff(removedTrack.Title, 70)}` dequeued.");
    }

    [SlashCommand("queue", "Shows to queue up to 16 tracks"), RequirePlayerJoined]
    public async Task QueueCommand()
    {
        await RespondWithEmbedAsync("Queue", (_player.Queue.Count == 0) ? "-- Empty --" : string.Join('\n', _player.Queue.Take(16).Select((track) => $"[{track.Title} - {track.Author}]({track.Url})")));
    }

    [SlashCommand("nowplaying", "Shows the currently playing tracks information"), RequirePlayerJoined]
    public async Task NowPlayingCommand()
    {
        string description = "Nothing is playing right now!";
        if (_player.Track is not null)
        {
            string position = _player.Track.IsStream ? "`🔴 LIVE `" : "─────────────────────────────".Insert((int)(_player.Track.Position.TotalSeconds / _player.Track.Duration.TotalSeconds * 30), "⚪");
            description = $"[{_player.Track.Title} - {_player.Track.Author}]({_player.Track.Url})\n```\n{position}\n[ {_player.Track.Position:hh\\:mm\\:ss} / {_player.Track.Duration:hh\\:mm\\:ss} ]```";
        }

        await RespondWithEmbedAsync("🎶 Now Playing", description, _player.Track is null ? null : await _player.Track.FetchArtworkAsync());
    }
}
