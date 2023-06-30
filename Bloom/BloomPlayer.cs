namespace Bloom;

public sealed class BloomPlayer : IAsyncDisposable
{
    private BloomNode _bloomNode;
    private PlayerState _state;
    private BloomQueue _queue;
    private int _volume;
    private string? _voiceSessionId;

    public PlayerState State => _state;
    public BloomQueue Queue => _queue;
    public BloomTrack? Track => _queue.CurrentTrack;
    public int Volume => _volume;
    public IVoiceChannel VoiceChannel { get; }
    public IMessageChannel TextChannel { get; }

    internal string? VoiceSessionId
    {
        get => _voiceSessionId;
        set => _voiceSessionId = value;
    }

    private string UpdatePlayerEndpoint => $"/v3/sessions/{_bloomNode.SessionId}/players/{VoiceChannel.GuildId}";

    internal BloomPlayer(BloomNode bloomNode, IVoiceChannel voiceChannel, IMessageChannel textChannel)
    {
        _bloomNode = bloomNode;
        _state = PlayerState.None;
        _queue = new BloomQueue();
        _volume = 100;
        VoiceChannel = voiceChannel;
        TextChannel = textChannel;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        await _queue.DisposeAsync();
        _bloomNode = null;
        _queue = null;
    }

    public Task PlayNextAsync()
    {
        _queue.Next();
        return PlayCurrentAsync();
    }

    public Task PlayPreviousAsync()
    {
        _queue.Previous();
        return PlayCurrentAsync();
    }

    public Task PlayCurrentAsync()
    {
        if (Track is null)
            throw new NullReferenceException(nameof(Track));

        _state = PlayerState.Playing;
        return UpdatePlayerAsync(new()
        {
            EncodedTrack = Track.Encoded,
        });
    }

    public Task StopAsync()
    {
        if (Track is null)
            throw new InvalidOperationException("The player isn't playing right now");

        _state = PlayerState.Stopped;
        return UpdatePlayerAsync(new()
        {
            EncodedTrack = "null",
        });
    }

    public Task PauseAsync()
    {
        if (State is not PlayerState.Playing)
            throw new InvalidOperationException("The player isn't playing right now");

        _state = PlayerState.Paused;
        return UpdatePlayerAsync(new()
        {
            Paused = true,
        });
    }

    public Task ResumeAsync()
    {
        if (State is not PlayerState.Paused)
            throw new InvalidOperationException("The player isn't paused right now");

        _state = PlayerState.Playing;
        return UpdatePlayerAsync(new()
        {
            Paused = false,
        });
    }

    public Task SeekAsync(TimeSpan position)
    {
        if (Track is null || _state is not PlayerState.Playing or PlayerState.Paused)
            throw new InvalidOperationException("Couldn't seek the current track 'cause it was null");

        return UpdatePlayerAsync(new()
        {
            Position = (long)position.TotalMilliseconds,
        });
    }

    public Task SetVolumeAsync(int volume)
    {
        if (volume is not (>= 0 and <= 1000))
            throw new InvalidOperationException($"The parameter {nameof(volume)} must be between 0 and 1000");

        _volume = volume;
        return UpdatePlayerAsync(new()
        {
            Volume = volume,
        });
    }

    public Task ApplyFilterAsync(IFilter filter, float volume, params EqualizerBand[] bands)
    {
        _volume = (int)(100 * volume);
        return UpdatePlayerAsync(new()
        {
            Filters = new FilterPayload(
                filter,
                volume,
                bands
            ),
        });
    }

    public Task ApplyFiltersAsync(IEnumerable<IFilter> filters, float volume, params EqualizerBand[] bands)
    {
        _volume = (int)(100 * volume);
        return UpdatePlayerAsync(new()
        {
            Filters = new FilterPayload(
                filters,
                volume,
                bands
            ),
        });
    }

    internal Task UpdatePlayerAsync(PlayerUpdatePayload updatePayload)
    {
        return _bloomNode.SendAsync(HttpMethod.Patch, UpdatePlayerEndpoint, updatePayload);
    }

    internal async Task ConnectAsync(bool selfDeaf, bool selfMute)
    {
        _state = PlayerState.Connected;
        await VoiceChannel.ConnectAsync(selfDeaf, selfMute, external: true);
    }

    internal Task UpdateSessionToConnectAsync(SocketVoiceServer voiceServer)
    {
        return UpdatePlayerAsync(new()
        {
            Voice = new VoiceStatePayload(
                token: voiceServer.Token,
                endpoint: voiceServer.Endpoint,
                sessionId: VoiceSessionId
            ),
        });
    }

    internal Task UpdateSessionIdAsync()
    {
        return UpdatePlayerAsync(new()
        {
            Voice = new VoiceStatePayload(
                token: null,
                endpoint: null,
                sessionId: VoiceSessionId
            ),
        });
    }

    internal async Task DisconnectAsync()
    {
        _state = PlayerState.Disconnected;
        await VoiceChannel.DisconnectAsync();
        await UpdateSessionToDisconnectAsync();
    }

    internal Task UpdateSessionToDisconnectAsync()
    {
        return _bloomNode.SendAsync(HttpMethod.Delete, UpdatePlayerEndpoint);
    }
}
