using System;
using System.Linq;
using System.Threading.Tasks;
using Bloom;
using Bloom.Filters;
using Bloom.Playback;
using Bloom.Searching;
using Blossom.AutoCompleteHandlers;
using Blossom.Preconditions;
using Blossom.Services;
using Blossom.Utilities;
using Discord;
using Discord.Interactions;

namespace Blossom.Modules;

public sealed class AudioModule : BaseInteractionModule
{
    private const int TrackPerPage = 10;
    private const int TrackTitleLimit = 70;
    private const string NowPlayingTitle = "🎶 Now Playing";
    private const string StreamDescription = "🔴 LIVE";
    private const string PositionBar = "─────────────────────────────";
    private const string PositionSign = "⚪";

    private readonly AudioService _audioService;

    public AudioModule(IServiceProvider services, AudioService audioService) : base(services)
    {
        _audioService = audioService;
    }

    [SlashCommand("join", "Joins the voice channel")]
    [RequirePlayerNotJoined, RequireVoiceChannel]
    public async Task JoinCommand()
    {
        IVoiceChannel voiceChannel = VoiceState!.VoiceChannel;
        await _audioService.JoinAsync(voiceChannel, Channel);
        await RespondAsync($"Joined to {voiceChannel.Mention}");
    }

    [SlashCommand("leave", "Leaves the voice channel")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task LeaveCommand()
    {
        IVoiceChannel voiceChannel = VoiceState!.VoiceChannel;
        await _audioService.LeaveAsync(voiceChannel.Guild);
        await RespondAsync($"Leaved from {voiceChannel.Mention}");
    }

    [SlashCommand("play", "Adds the track(s) with given query")]
    [RequireUserInVoiceChannel, RequireVoiceChannel]
    public async Task PlayCommand([Summary(description: "The track's URL or name to search")] string query)
    {
        string response = string.Empty;
        BloomPlayer player = await _audioService.GetOrJoinAsync(VoiceState!.VoiceChannel, Channel);
        if (player.State is PlayerState.Connected)
            response = $"Joined to {player.VoiceChannel.Mention}\n";

        LoadResult result = await _audioService.SearchAsync(query);
        if (result.Kind is LoadResultKind.Empty or LoadResultKind.Error)
        {
            await RespondAsync($"{response}Could't find any tracks with given query!");
            return;
        }

        if (result.Kind is LoadResultKind.Playlist)
        {
            PlaylistLoadResult playlistResult = (PlaylistLoadResult)result;
            player.Queue.Add(playlistResult.Tracks);
            await RespondAsync($"{response}Playlist `{playlistResult.Name}` loaded with {playlistResult.Tracks.Count} tracks.");
        }
        else if (result.Kind is LoadResultKind.Track)
        {
            TrackLoadResult trackResult = (TrackLoadResult)result;
            player.Queue.Add(trackResult.Track);
            await RespondAsync($"{response}`{trackResult.Track.Title.EndAt(TrackTitleLimit)}` queued.");
        }
        else
        {
            SearchLoadResult searchResult = (SearchLoadResult)result;
            player.Queue.Add(searchResult.Tracks[0]);
            await RespondAsync($"{response}`{searchResult.Tracks[0].Title.EndAt(TrackTitleLimit)}` queued.");
        }

        if (player.State is PlayerState.Connected or PlayerState.Stopped)
            await player.PlayNextAsync();
    }

    [SlashCommand("insert", "Inserts the track(s) with given query to the provided position")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task InsertCommand(
        [Summary(description: "The track's URL or name to search")] string query,
        [Summary(description: "The position to be inserted"), MinValue(1)] int position
    )
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;

        LoadResult result = await _audioService.SearchAsync(query);
        if (result.Kind is LoadResultKind.Empty or LoadResultKind.Error)
        {
            await RespondAsync("Could't find any tracks with given query!");
            return;
        }

        position = Math.Min(position, player.Queue.Count) - 1;

        if (result.Kind is LoadResultKind.Playlist)
        {
            PlaylistLoadResult playlistResult = (PlaylistLoadResult)result;
            foreach (BloomTrack trackToInsert in playlistResult.Tracks.Reverse())
            {
                player.Queue.InsertAt(trackToInsert, position);
            }
            await RespondAsync($"Playlist `{playlistResult.Name}` loaded with {playlistResult.Tracks.Count} tracks.");
        }
        else if (result.Kind is LoadResultKind.Track)
        {
            TrackLoadResult trackResult = (TrackLoadResult)result;
            player.Queue.InsertAt(trackResult.Track, position);
            await RespondAsync($"`{trackResult.Track.Title.EndAt(TrackTitleLimit)}` queued.");
        }
        else
        {
            SearchLoadResult searchResult = (SearchLoadResult)result;
            player.Queue.InsertAt(searchResult.Tracks[0], position);
            await RespondAsync($"`{searchResult.Tracks[0].Title.EndAt(TrackTitleLimit)}` queued.");
        }
    }

    [SlashCommand("remove", "Removes a track from the queue")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task RemoveCommand([Summary(description: "The position of track"), MinValue(1), Autocomplete(typeof(RemoveTrackAutocompleteHandler))] int position)
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
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
        await RespondAsync($"`{removedTrack.Title.EndAt(TrackTitleLimit)}` removed from the queue.");
    }

    [SlashCommand("stop", "Stops the player and clears the queue")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task StopCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        await player.StopAsync();
        player.Queue.Clear();
        await RespondAsync("Player stopped and queue cleared.");
    }

    [SlashCommand("pause", "Pauses the player")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequirePlayerState(PlayerState.Playing)]
    public async Task PauseCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        await player.PauseAsync();
        await RespondAsync("Player paused.");
    }

    [SlashCommand("resume", "Resumes the player")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequirePlayerState(PlayerState.Paused)]
    public async Task ResumeCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        await player.ResumeAsync();
        await RespondAsync("Player resumed.");
    }

    [SlashCommand("volume", "Sets the player's volume")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task VolumeCommand([Summary(description: "The volume value"), MinValue(0), MaxValue(200)] ushort volume)
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        await player.SetVolumeAsync(volume);
        await RespondAsync($"Volume setted to `{volume}`.");
    }

    [SlashCommand("seek", "Seeks the current track to given position (mm:ss)")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequirePlayerState(PlayerState.Playing)]
    public async Task SeekAsync([Summary(description: "The position to seek")] TimeSpan position)
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;

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
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        player.Queue.LoopMode = player.Queue.LoopMode.Next();
        await RespondAsync($"Loop mode set to: `{player.Queue.LoopMode}`");
    }

    [SlashCommand("next", "Stops the current track and plays next one if exists")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task NextCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
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
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task PreviousCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
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
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        if (player.State is PlayerState.Playing)
            await player.SeekAsync(TimeSpan.Zero);
        else
            await player.PlayCurrentAsync();

        await RespondAsync("Rewinded current track.");
    }

    [SlashCommand("filter", "Applies a filter preset to playback")]
    [RequirePlayerJoined, RequireUserInVoiceChannel]
    public async Task FilterCommand([Summary(description: "The name of the preset"), Autocomplete(typeof(FilterAutocompleteHandler))] string name)
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        FilterPreset? preset = _audioService.GetFilterPreset(name);
        if (preset is null)
        {
            await RespondAsync("Preset couldn't found with provided name!");
            return;
        }

        await player.ApplyFiltersAsync(preset.Filters, preset.Volume, preset.Equalizer);
        await RespondAsync($"{preset.Name} applied. It may take a few seconds to change.");
    }

    [SlashCommand("shuffle", "Shuffles the queue")]
    [RequirePlayerJoined, RequireUserInVoiceChannel, RequireNonEmptyQueue]
    public async Task ShuffleCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        player.Queue.Shuffle();
        await RespondAsync("Queue shuffled.");
    }

    [SlashCommand("queue", "Show the queue")]
    [RequirePlayerJoined, RequireNonEmptyQueue]
    public async Task QueueCommand([Summary(description: "The page number of the queue (zero-indexed)")] int? page = null)
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        page ??= player.Queue.Current / TrackPerPage;
        int startIndex = page.Value * TrackPerPage;
        BloomTrack[] tracks = player.Queue.Take(startIndex..(startIndex + TrackPerPage)).Where((track) => track is not null).ToArray();

        Embed embed = EmbedUtility.CreateEmbed(
            title: $"-- Queue --",
            description: string.Join('\n', tracks
                    .Select((track, index) =>
                        $"{startIndex + index + 1}) [{track.Title}]({track.Url}){((startIndex + index == player.Queue.Current) ? " (Now Playing)" : string.Empty)}"
                    )
                ),
            color: Cherry,
            footer: EmbedUtility.CreateFooter($"Page {page}")
        );

        await RespondAsync(embed: embed);
    }

    [SlashCommand("nowplaying", "Shows the currently playing track's information")]
    [RequirePlayerJoined, RequirePlayerState(PlayerState.Playing)]
    public async Task NowPlayingCommand()
    {
        BloomPlayer player = _audioService.GetPlayer(Guild)!;

        string description = $"[{player.Track!.Title}]({player.Track.Url})\n";
        if (player.Track.IsStream)
        {
            description += $"{StreamDescription}\n";
        }
        else
        {
            description += $"```\n{PositionBar.Insert((int)(player.Track.Position.TotalSeconds / player.Track.Duration.TotalSeconds * 30), PositionSign)}\n\n";
            description += $"[ {player.Track.Position:hh\\:mm\\:ss} / {player.Track.Duration:hh\\:mm\\:ss} ]```";
        }

        Embed embed = EmbedUtility.CreateEmbed(
            title: NowPlayingTitle,
            description: description,
            thumbnail: player.Track.ArtworkUrl,
            color: Cherry
        );

        await RespondAsync(embed: embed);
    }

    [SlashCommand("lyrics", "Shows the lyrics for current track")]
    [RequirePlayerJoined]
    public async Task LyricsCommand()
    {
        await DeferAsync();

        BloomPlayer player = _audioService.GetPlayer(Guild)!;
        if (player.Track is null || player.State is not PlayerState.Playing or PlayerState.Paused)
        {
            await FollowupAsync("Nothing is playing right now!");
            return;
        }

        LyricsResult? lyrics = await _audioService.GetLyricsAsync(player.Track);
        if (lyrics is null)
        {
            await FollowupAsync("Couldn't find lyrics for this song!");
            return;
        }

        Embed embed = EmbedUtility.CreateEmbed(
            title: $"Lyrics for {player.Track.Title.EndAt(TrackTitleLimit)}",
            description: lyrics.Lyrics,
            color: Cherry
        );

        await FollowupAsync(embed: embed);
    }
}
