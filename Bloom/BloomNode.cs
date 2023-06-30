namespace Bloom;

public sealed class BloomNode : IAsyncDisposable
{
    private const string LoadTracksEndpoint = "/v3/loadtracks?identifier={0}";

    private readonly DiscordSocketClient _discordClient;
    private readonly BloomConfig _config;
    private int _reconnectAttemps;
    private int _reconnectDelay;
    private readonly ClientWebSocket _webSocket;
    private readonly HttpClient _httpClient;
    private readonly CancellationTokenSource _connectionTokenSource;
    private readonly Dictionary<ulong, BloomPlayer> _players;
    private CancellationTokenSource? _cancellationTokenSource;

    public event NodeExceptionEvent? NodeException;
    public event TrackStartEvent? TrackStarted;
    public event TrackEndEvent? TrackEnded;
    public event TrackExceptionEvent? TrackException;
    public event TrackStuckEvent? TrackStucked;

    public bool IsConnected { get; private set; }

    internal string SessionId { get; private set; }

    public BloomNode(DiscordSocketClient discordClient, BloomConfig config)
    {
        _discordClient = discordClient;
        _config = config;
        _webSocket = new ClientWebSocket();
        _httpClient = new HttpClient();
        _connectionTokenSource = new CancellationTokenSource();
        _players = new Dictionary<ulong, BloomPlayer>();
        IsConnected = false;
        SessionId = string.Empty;
    }

    public async ValueTask DisposeAsync()
    {
        if (IsConnected)
            await DisconnectAsync();

        _connectionTokenSource?.Dispose();
        _webSocket.Dispose();
    }

    public async Task ConnectAsync()
    {
        if (_webSocket.State is WebSocketState.Open)
            throw new InvalidOperationException("The node already in open state");

        _discordClient.VoiceServerUpdated += VoiceServerUpdated;
        _discordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;

        _webSocket.Options.SetRequestHeader("User-Id", _discordClient.CurrentUser.Id.ToString());
        _webSocket.Options.SetRequestHeader("Client-Name", $"{nameof(Bloom)}/{typeof(BloomNode).Assembly.GetName().Version}");
        _webSocket.Options.SetRequestHeader("Num-Shards", _config.ShardCount.ToString());
        _webSocket.Options.SetRequestHeader("Authorization", _config.Authorization);

        _httpClient.DefaultRequestHeaders.Add("Authorization", _config.Authorization);

        async Task VerifyConnectionAsync(Task task)
        {
            if (task.Exception is not null)
            {
                await ReconnectAsync();
                return;
            }

            IsConnected = true;
            await StartReceivingAsync();
        }

        try
        {
            await _webSocket
                .ConnectAsync(new Uri(_config.Endpoint), _connectionTokenSource.Token)
                .ContinueWith(VerifyConnectionAsync);
        }
        catch (Exception exception)
        {
            if (exception is not ObjectDisposedException)
                return;

            NodeException?.Invoke(new NodeExceptionEventArgs(exception.Message));
            await ReconnectAsync();
        }
    }

    public async Task DisconnectAsync()
    {
        if (_webSocket.State is not WebSocketState.Open)
            throw new InvalidOperationException("The node is not in open state");

        try
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect Request", _connectionTokenSource.Token);
        }
        catch (Exception exception)
        {
            NodeException?.Invoke(new NodeExceptionEventArgs(exception.Message));
        }
        finally
        {
            IsConnected = false;
            _connectionTokenSource.Cancel(false);
        }
    }

    public BloomPlayer? GetPlayer(IGuild guild)
    {
        _ = _players.TryGetValue(guild.Id, out BloomPlayer? player);
        return player;
    }

    public bool HasPlayer(IGuild guild)
    {
        return _players.ContainsKey(guild.Id);
    }

    public async Task<BloomPlayer> JoinAsync(IVoiceChannel voiceChannel, IMessageChannel textChannel)
    {
        if (!IsConnected)
            throw new InvalidOperationException($"The {nameof(BloomNode)} isn't connected to the remote server");

        var player = new BloomPlayer(this, voiceChannel, textChannel);
        _players.Add(voiceChannel.GuildId, player);
        await player.ConnectAsync(_config.SelfDeaf, _config.SelfMute);
        return player;
    }

    public async Task LeaveAsync(IVoiceChannel voiceChannel)
    {
        if (!IsConnected)
            throw new InvalidOperationException($"The {nameof(BloomNode)} isn't connected to the remote server");

        BloomPlayer? player = GetPlayer(voiceChannel.Guild)
            ?? throw new InvalidOperationException($"There is no player in {voiceChannel.Mention}");

        _players.Remove(voiceChannel.GuildId);
        await player.DisposeAsync();
    }

    public async Task LeaveAsync(IGuild guild)
    {
        if (!IsConnected)
            throw new InvalidOperationException($"The {nameof(BloomNode)} isn't connected to the remote server");

        BloomPlayer? player = GetPlayer(guild)
            ?? throw new InvalidOperationException($"There is no player for {guild.Name}");

        _players.Remove(guild.Id);
        await player.DisposeAsync();
    }

    public async Task<SearchResult> SearchAsync(string query, SearchKind kind)
    {
        if (!IsConnected)
            throw new InvalidOperationException($"The {nameof(BloomNode)} isn't connected to the remote server");

        query = kind switch
        {
            SearchKind.YouTube => $"ytsearch:{query}",
            SearchKind.YouTubeMusic => $"ytmsearch:{query}",
            SearchKind.SoundCloud => $"scsearch:{query}",
            _ => query,
        };

        JsonNode data = await RequestAsync(HttpMethod.Get, string.Format(LoadTracksEndpoint, query));
        return SearchResult.Parse(data);
    }

    internal async Task SendAsync(HttpMethod method, string uri, IPayload? payload = null)
    {
        using HttpRequestMessage requestMessage = new(method, $"{_config.RestEndpoint}{uri}");
        if (payload is not null)
        {
            string jsonString = JsonSerializer.Serialize(payload, payload.GetType())
                .Replace("\"null\"", "null");
            requestMessage.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
        if (!responseMessage.IsSuccessStatusCode)
        {
            string response = await responseMessage.Content.ReadAsStringAsync();
            NodeException?.Invoke(new NodeExceptionEventArgs(response));
        }
    }

    internal async Task<JsonNode> RequestAsync(HttpMethod method, string uri, IPayload? payload = null)
    {
        using HttpRequestMessage requestMessage = new(method, $"{_config.RestEndpoint}{uri}");
        if (payload is not null)
        {
            string jsonString = JsonSerializer.Serialize(payload, payload.GetType())
                .Replace("\"null\"", "null");
            requestMessage.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
        string response = await responseMessage.Content.ReadAsStringAsync();
        if (!responseMessage.IsSuccessStatusCode)
            NodeException?.Invoke(new NodeExceptionEventArgs(response));

        return JsonNode.Parse(response)!;
    }

    private async Task ReconnectAsync()
    {
        if (0 <= _config.ReconnectAttemps || _config.ReconnectAttemps <= _reconnectAttemps)
            return;

        _connectionTokenSource.Cancel(false);
        _reconnectAttemps++;
        _reconnectDelay += _config.ReconnectDelayInMiliseconds;

        Console.WriteLine($"Trying to connect in {_reconnectDelay} seconds");
        await Task.Delay(_reconnectDelay);
        await ConnectAsync();
    }

    private async Task StartReceivingAsync()
    {
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                string jsonString = await ReceiveAsync();
                JsonNode data = JsonNode.Parse(jsonString)!;
                IGuild guild;
                BloomPlayer player;
                switch (data["op"]!.ToString())
                {
                    case "ready":
                        SessionId = data["sessionId"]!.ToString();
                        break;

                    case "playerUpdate":
                        if (bool.Parse(data["state"]!["connected"]!.ToString()))
                        {
                            guild = _discordClient.GetGuild(ulong.Parse(data["guildId"]!.ToString()));
                            player = GetPlayer(guild)!;
                            player.Track?.UpdatePosition(long.Parse(data["state"]!["position"]!.ToString()));
                        }
                        break;

                    case "stats":
                        break;

                    case "event":
                        guild = _discordClient.GetGuild(ulong.Parse(data["guildId"]!.ToString()));
                        player = GetPlayer(guild)!;
                        switch (data["type"]!.ToString())
                        {
                            case "TrackStartEvent":
                                TrackStarted?.Invoke(new TrackStartEventArgs(player));
                                break;

                            case "TrackEndEvent":
                                TrackEnded?.Invoke(new TrackEndEventArgs(player,
                                    data["reason"]!.ToString() switch
                                    {
                                        "FINISHED" => TrackEndReason.Finished,
                                        "LOAD_FAILED" => TrackEndReason.LoadFailed,
                                        "STOPPED" => TrackEndReason.Stopped,
                                        "REPLACED" => TrackEndReason.Replaced,
                                        "CLEANUP" => TrackEndReason.Cleanup,
                                        _ => TrackEndReason.None,
                                    }
                                ));
                                break;

                            case "TrackExceptionEvent":
                                TrackException?.Invoke(new TrackExceptionEventArgs(player, new TrackEventException(
                                    data["exception"]!["message"]!.ToString(),
                                    data["exception"]!["cause"]!.ToString(),
                                    Enum.Parse<TrackEventExceptionSeverity>(data["exception"]!["severity"]!.ToString(), ignoreCase: true)
                                )));
                                break;

                            case "TrackStuckEvent":
                                TrackStucked?.Invoke(new TrackStuckEventArgs(player, TimeSpan.FromMilliseconds(int.Parse(data["thresholdMs"]!.ToString()))));
                                break;

                            case "WebSocketClosedEvent":
                                break;

                            default:
                                throw new Exception("Unhandled received event type");
                        }
                        break;

                    default:
                        throw new Exception("Unhandled received data type");
                }
            }
            catch
            {
                throw;
            }
        }
    }

    private async ValueTask<string> ReceiveAsync()
    {
        byte[] buffer = new byte[128 * 1024];
        int length = 0;

        while (true)
        {
            WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, length, 8192), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                IsConnected = false;
                break;
            }

            length += result.Count;
            if (result.EndOfMessage)
                break;
        }

        return Encoding.UTF8.GetString(buffer, 0, length);
    }

    private async Task VoiceServerUpdated(SocketVoiceServer voiceServer)
    {
        if (GetPlayer(voiceServer.Guild.Value) is BloomPlayer player)
        {
            await player.UpdateSessionToConnectAsync(voiceServer);
        }
    }

    private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        if (user.Id == _discordClient.CurrentUser.Id)
        {
            if (after.VoiceChannel is not null)
            {
                BloomPlayer player = GetPlayer(after.VoiceChannel.Guild)!;
                player.VoiceSessionId = after.VoiceSessionId;
                if (player.VoiceChannel != after.VoiceChannel)
                {
                    await player.UpdateSessionIdAsync();
                }
            }
            else if (HasPlayer(before.VoiceChannel.Guild))
            {
                await LeaveAsync(before.VoiceChannel);
            }
        }
        else if (!user.IsBot)
        {
            if (before.VoiceChannel is null && after.VoiceChannel is not null)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            else if (before.VoiceChannel is not null && after.VoiceChannel is null)
            {
                if (!HasPlayer(before.VoiceChannel.Guild))
                    return;

                if (before.VoiceChannel.Guild.VoiceChannels.Any(
                        static (voiceChannel) => voiceChannel.ConnectedUsers.Any(
                            static (connectedUser) => !connectedUser.IsBot
                        )
                    )
                )
                    return;

                _cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(_config.LeaveDelayInMiliseconds, _cancellationTokenSource.Token);
                        await LeaveAsync(before.VoiceChannel.Guild);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }, _cancellationTokenSource.Token);
            }
        }
    }
}
