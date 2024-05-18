using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bloom.Filters;
using Bloom.Payloads;
using Discord;

namespace Bloom.Playback;

/// <summary>
/// Represents a player for playing audio tracks in a voice channel.
/// </summary>
public sealed class BloomPlayer : IAsyncDisposable
{
    /// <summary>
    /// The minimum volume value.
    /// </summary>
    public const int MinVolume = 0;

    /// <summary>
    /// The maximum volume value.
    /// </summary>
    public const int MaxVolume = 1000;

    /// <summary>
    /// The default volume value.
    /// </summary>
    public const int DefaultVolume = 100;

    /// <summary>
    /// Gets the <see cref="BloomNode"/> that the player is belong to.
    /// </summary>
    public BloomNode Node { get; }

    /// <summary>
    /// Gets the <see cref="BloomQueue"/> of the player.
    /// </summary>
    public BloomQueue Queue { get; }

    /// <summary>
    /// Gets the current <see cref="BloomTrack"/> in the queue.
    /// </summary>
    public BloomTrack? Track => Queue.CurrentTrack;

    /// <summary>
    /// Gets the state of the player.
    /// </summary>
    public PlayerState State { get; private set; }

    /// <summary>
    /// Gets the volume of the player. The value must be between <see cref="MinVolume"/> and <see cref="MaxVolume"/>.
    /// </summary>
    public int Volume { get; private set; }

    /// <summary>
    /// Gets the voice channel where the player is connected to.
    /// </summary>
    public IVoiceChannel VoiceChannel { get; internal set; }

    /// <summary>
    /// Gets the text channel where the player is connected to.
    /// </summary>
    public IMessageChannel TextChannel { get; internal set; }

    /// <summary>
    /// Gets the voice session identifier of the player.
    /// </summary>
    public string VoiceSessionId { get; internal set; }

    internal BloomPlayer(BloomNode node, IVoiceChannel voiceChannel, IMessageChannel textChannel)
    {
        Node = node;
        Queue = [];
        State = PlayerState.Disconnected;
        Volume = DefaultVolume;
        VoiceChannel = voiceChannel;
        TextChannel = textChannel;
        VoiceSessionId = string.Empty;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (State is PlayerState.Connected or PlayerState.Playing or PlayerState.Paused)
            await DisconnectAsync();

        Queue.Clear();
    }

    /// <summary>
    /// Plays the next track in the queue.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    public async ValueTask PlayNextAsync()
    {
        Queue.MoveNext();
        await PlayCurrentAsync();
    }

    /// <summary>
    /// Plays the previous track in the queue.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    public async ValueTask PlayPreviousAsync()
    {
        Queue.MovePrevious();
        await PlayCurrentAsync();
    }

    /// <summary>
    /// Plays the current track in the queue.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="NullReferenceException"></exception>
    public async ValueTask PlayCurrentAsync()
    {
        if (Track is null)
            throw new NullReferenceException(nameof(Track));

        State = PlayerState.Playing;
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            EncodedTrack = Track.Encoded,
        });
    }

    /// <summary>
    /// Stops the playback.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask StopAsync()
    {
        if (State is not PlayerState.Playing or PlayerState.Paused)
            throw new InvalidOperationException("The player is not playing right now");

        State = PlayerState.Stopped;
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            EncodedTrack = "null",
        });
    }

    /// <summary>
    /// Pauses the playback.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask PauseAsync()
    {
        if (State is not PlayerState.Playing)
            throw new InvalidOperationException("The player is not playing right now");

        State = PlayerState.Paused;
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            Paused = true,
        });
    }

    /// <summary>
    /// Resumes the playback.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask ResumeAsync()
    {
        if (State is not PlayerState.Paused)
            throw new InvalidOperationException("The player is not paused right now");

        State = PlayerState.Playing;
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            Paused = false,
        });
    }

    /// <summary>
    /// Seeks to the specified position in the current track.
    /// </summary>
    /// <param name="position">The position to seek to.</param>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask SeekAsync(TimeSpan position)
    {
        if (State is not PlayerState.Playing or PlayerState.Paused)
            throw new InvalidOperationException("Couldn't seek while the player isn't playing or paused");

        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            Position = (long)position.TotalMilliseconds,
        });
    }

    /// <summary>
    /// Sets the volume of the player. The value must be between <see cref="MinVolume"/> and <see cref="MaxVolume"/>.
    /// </summary>
    /// <param name="volume">The volume value to set.</param>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async ValueTask SetVolumeAsync(int volume)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(volume, MinVolume, nameof(volume));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(volume, MaxVolume, nameof(volume));

        Volume = volume;
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            Volume = volume,
        });
    }

    /// <summary>
    /// Applies the specified filter to the player.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <param name="volume">The volume amplification to apply.</param>
    /// <param name="bands">The equalizer bands to apply.</param>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    public async ValueTask ApplyFilterAsync(IFilter filter, float volume, params EqualizerBand[] bands)
    {
        Volume = (int)(Volume * volume);
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            Filters = new FilterPayload(filter, volume, bands),
        });
    }

    /// <summary>
    /// Applies the specified filters to the player.
    /// </summary>
    /// <param name="filters">The filters to apply.</param>
    /// <param name="volume">The volume amplification to apply.</param>
    /// <param name="bands">The equalizer bands to apply.</param>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    public async ValueTask ApplyFiltersAsync(IReadOnlyList<IFilter> filters, float volume, params EqualizerBand[] bands)
    {
        Volume = (int)(Volume * volume);
        await Node.UpdatePlayerAsync(this, new PlayerUpdatePayload
        {
            Filters = new FilterPayload(filters, volume, bands),
        });
    }

    internal async ValueTask ConnectAsync(bool selfDeaf, bool selfMute)
    {
        await VoiceChannel.ConnectAsync(selfDeaf: selfDeaf, selfMute: selfMute, external: true);
        State = PlayerState.Connected;
    }

    internal async Task DisconnectAsync()
    {
        await VoiceChannel.DisconnectAsync();
        State = PlayerState.Disconnected;
    }
}
