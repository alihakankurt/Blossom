namespace Blossom.Modules;

public sealed class AudioModule : BaseInteractionModule
{
    private const int TrackPerPage = 10;
    private const int TrackTitleLimit = 70;
    private const string NowPlayingTitle = "🎶 Now Playing";
    private const string StreamDescription = "🔴 LIVE";
    private const string PositionBar = "─────────────────────────────";
    private const string PositionSign = "⚪";

    private readonly BloomNode _bloomNode;

    public AudioModule(IServiceProvider services, BloomNode bloomNode) : base(services)
    {
        _bloomNode = bloomNode;
    }

    [SlashCommand("join", "Joins the voice channel")]
    [RequirePlayerNotJoined, RequireVoiceChannel]
    public async Task JoinCommand()
    {
        IVoiceChannel voiceChannel = VoiceState!.VoiceChannel;
        await _bloomNode.JoinAsync(voiceChannel, Channel);
        await RespondAsync($"Joined to {voiceChannel.Mention}");
    }

    [SlashCommand("leave", "Leaves the voice channel")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task LeaveCommand()
    {
        IVoiceChannel voiceChannel = VoiceState!.VoiceChannel;
        await _bloomNode.LeaveAsync(voiceChannel);
        await RespondAsync($"Leaved from {voiceChannel.Mention}");
    }

    [SlashCommand("play", "Adds the track(s) with given query")]
    [RequireUserInVoiceChannel, RequireVoiceChannel]
    public async Task PlayCommand([Summary(description: "The track's URL or name to search")] string query)
    {
        string response = string.Empty;
        BloomPlayer? player = _bloomNode.GetPlayer(Guild);
        if (player is null)
        {
            player = await _bloomNode.JoinAsync(VoiceState!.VoiceChannel, Channel);
            response = $"Joined to {player.VoiceChannel.Mention}\n";
        }

        query = query.Trim('<', '>');
        bool isWellFormed = Uri.IsWellFormedUriString(query, UriKind.Absolute);
        SearchResult result = await _bloomNode.SearchAsync(query, isWellFormed ? SearchKind.Direct : SearchKind.YouTube);
        if (result.Kind is SearchResultKind.NoMatches or SearchResultKind.LoadFailed)
        {
            await RespondAsync($"{response}Could't find any tracks with given query!");
            return;
        }

        if (result.Kind is SearchResultKind.PlaylistLoaded)
        {
            player.Queue.AddRange(result.Tracks!);
            await RespondAsync($"{response}Playlist `{result.Playlist!.Name}` loaded with {result.Tracks!.Count} tracks.");
        }
        else
        {
            BloomTrack trackToAdd = result.Tracks![0];
            player.Queue.Add(trackToAdd);
            await RespondAsync($"{response}`{trackToAdd.Title.CutOff(TrackTitleLimit)}` queued.");
        }

        if (player.State is PlayerState.None or PlayerState.Connected or PlayerState.Stopped)
            await player.PlayNextAsync();
    }

    [SlashCommand("insert", "Inserts the track(s) with given query to the provided position")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task InsertCommand(
        [Summary(description: "The track's URL or name to search")] string query,
        [Summary(description: "The position to be inserted"), MinValue(1)] int position
    )
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;

        query = query.Trim('<', '>');
        bool isWellFormed = Uri.IsWellFormedUriString(query, UriKind.Absolute);
        SearchResult result = await _bloomNode.SearchAsync(query, isWellFormed ? SearchKind.Direct : SearchKind.YouTube);
        if (result.Kind is SearchResultKind.NoMatches or SearchResultKind.LoadFailed)
        {
            await RespondAsync("Could't find any tracks with given query!");
            return;
        }

        position = Math.Min(position - 1, player.Queue.Count);
        if (result.Kind is SearchResultKind.PlaylistLoaded)
        {
            foreach (BloomTrack trackToInsert in result.Tracks!.Reverse())
            {
                player.Queue.InsertAt(trackToInsert, position);
            }
            await RespondAsync($"Playlist `{result.Playlist!.Name}` loaded with {result.Tracks!.Count} tracks.");
        }
        else
        {
            BloomTrack trackToInsert = result.Tracks![0];
            player.Queue.InsertAt(trackToInsert, position);
            await RespondAsync($"`{trackToInsert.Author} - {trackToInsert.Title.CutOff(TrackTitleLimit)}` queued.");
        }
    }

    [SlashCommand("remove", "Removes a track from the queue")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task RemoveCommand([Summary(description: "The position of track"), MinValue(1), AutoComplete<RemoveTrackAutocompleteHandler>()] int position)
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        if (position-- > player.Queue.Count)
        {
            await RespondAsync($"The provided positon must be lesser than or equal to queue's track count ({player.Queue.Count})!");
            return;
        }

        if (position == player.Queue.Current)
        {
            await RespondAsync("You can't remove the track that currently playing!");
            return;
        }

        BloomTrack removedTrack = player.Queue.RemoveAt(position);
        await RespondAsync($"`{removedTrack.Title.CutOff(TrackTitleLimit)}` removed from the queue.");
    }

    [SlashCommand("stop", "Stops the player and clears the queue")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task StopCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        await player.StopAsync();
        player.Queue.Clear();
        await RespondAsync("Player stopped and queue cleared.");
    }

    [SlashCommand("pause", "Pauses the player")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, EnsurePlayerState(PlayerState.Playing)]
    public async Task PauseCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        await player.PauseAsync();
        await RespondAsync("Player paused.");
    }

    [SlashCommand("resume", "Resumes the player")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, EnsurePlayerState(PlayerState.Paused)]
    public async Task ResumeCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        await player.ResumeAsync();
        await RespondAsync("Player resumed.");
    }

    [SlashCommand("volume", "Sets the player's volume")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task VolumeCommand([Summary(description: "The volume value"), MinValue(0), MaxValue(200)] ushort volume)
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        await player.SetVolumeAsync(volume);
        await RespondAsync($"Volume setted to `{volume}`.");
    }

    [SlashCommand("seek", "Seeks the current track to given position (mm:ss)")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, EnsurePlayerState(PlayerState.Playing)]
    public async Task SeekAsync([Summary(description: "The position to seek")] TimeSpan position)
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;

        if (player.Track!.Duration.CompareTo(position) < 1)
        {
            await RespondAsync("The `position` can't exceed the current track's duration!");
            return;
        }

        await player.SeekAsync(position);
        await RespondAsync($"Seeked to `{position:mm':'ss}`.");
    }


    [SlashCommand("loop", "Changes the loop mode of the player")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task LoopCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        player.Queue.Loop();
        await RespondAsync($"Loop mode set to: `{player.Queue.LoopMode}`");
    }

    [SlashCommand("next", "Stops the current track and plays next one if exists")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue, EnsureNotLoopMode(LoopMode.One)]
    public async Task NextCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        string response = "There must be some bug!";
        if (player.State is PlayerState.Playing or PlayerState.Paused)
        {
            await player.StopAsync();
            response = "Player stopped.\n";
        }

        if (player.Queue.HasNext)
        {
            await player.PlayNextAsync();
            response = "Playing next track.";
        }

        await RespondAsync(response);
    }

    [SlashCommand("previous", "Stops the current track and plays previous one if exists")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue, EnsureNotLoopMode(LoopMode.One)]
    public async Task PreviousCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        string response = "There must be some bug!";
        if (player.State is PlayerState.Playing or PlayerState.Paused)
        {
            await player.StopAsync();
            response = "Player stopped.";
        }

        if (player.Queue.HasPrevious)
        {
            await player.PlayPreviousAsync();
            response = "Playing previous track.";
        }

        await RespondAsync(response);
    }

    [SlashCommand("rewind", "Plays the current track from start")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task RewindCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        if (player.State is PlayerState.Playing)
            await player.SeekAsync(TimeSpan.Zero);
        else
            await player.PlayCurrentAsync();

        await RespondAsync("Rewinded current track.");
    }

    [SlashCommand("filter", "Applies a filter preset to playback")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task FilterCommand([Summary(description: "The name of the preset"), AutoComplete<FilterAutoCompleteHandler>()] string name)
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        FilterPreset? preset = AudioService.GetFilterPreset(name);
        if (preset is null)
        {
            await RespondAsync("Preset couldn't found with provided name!");
            return;
        }

        await player.ApplyFiltersAsync(preset.Filters, preset.Volume, preset.Bands);
        await RespondAsync($"{preset.Name} applied.");
    }

    [SlashCommand("shuffle", "Shuffles the queue")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task ShuffleCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        player.Queue.Shuffle();
        await RespondAsync("Queue shuffled.");
    }

    [SlashCommand("queue", "Show the queue")]
    [RequirePlayerJoined, RequireNonEmptyQueue]
    public async Task QueueCommand([Summary(description: "The page number of the queue (zero-indexed)")] int? page = null)
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        page ??= player.Queue.Current / TrackPerPage;
        int startIndex = page.Value * TrackPerPage;
        BloomTrack[] tracks = player.Queue.Take(startIndex..(startIndex + TrackPerPage)).Where((track) => track is not null).ToArray();
        await RespondWithEmbedAsync(
            title: $"-- Queue --",
            description: string.Join('\n', tracks
                    .Select((track, index) =>
                        $"{startIndex + index + 1}) [{track.Title}]({track.Url}){((startIndex + index == player.Queue.Current) ? " (Now Playing)" : string.Empty)}"
                    )
                ),
            footer: CreateFooter($"Page {page}")
        );
    }

    [SlashCommand("nowplaying", "Shows the currently playing track's information")]
    [RequirePlayerJoined, EnsurePlayerState(PlayerState.Playing)]
    public async Task NowPlayingCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;

        string description = $"[{player.Track!.Title}]({player.Track.Url})\n";
        if (player.Track.IsStream)
        {
            await RespondWithEmbedAsync(
                title: NowPlayingTitle,
                description: $"{description}```\n{StreamDescription}```"
            );
            return;
        }

        await RespondWithEmbedAsync(
            title: NowPlayingTitle,
            description: description +
                $"```\n{PositionBar.Insert((int)(player.Track.Position.TotalSeconds / player.Track.Duration.TotalSeconds * 30), PositionSign)}\n\n"
              + $"[ {player.Track.Position:hh\\:mm\\:ss} / {player.Track.Duration:hh\\:mm\\:ss} ]```"
        );
    }

    [SlashCommand("lyrics", "Shows the lyrics for current track")]
    [RequirePlayerJoined]
    public async Task LyricsCommand()
    {
        BloomPlayer player = _bloomNode.GetPlayer(Guild)!;
        if (player.Track is null || player.State is not PlayerState.Playing or PlayerState.Paused)
        {
            await RespondAsync("Nothing is playing right now!");
            return;
        }

        string? lyrics = await player.Track.GetLyricsAsync();
        if (lyrics is null)
        {
            await RespondAsync("Couldn't find lyrics for this song!");
            return;
        }

        await RespondWithEmbedAsync(
            title: $"Lyrics for {player.Track.Title.CutOff(TrackTitleLimit)}",
            description: lyrics
        );
    }
}
