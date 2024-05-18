using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Bloom.Events;
using Bloom.Parsing;
using Bloom.Payloads;
using Bloom.Playback;
using Bloom.Searching;
using Discord;
using Discord.WebSocket;

namespace Bloom;

/// <summary>
/// Represents a node that connects to the Lavalink server.
/// </summary>
public sealed class BloomNode : IAsyncDisposable
{
    private const string LoadTracksEndpoint = "loadtracks?identifier={0}";
    private const string UpdatePlayerEndpoint = "sessions/{0}/players/{1}";

    private readonly DiscordSocketClient _discordClient;
    private readonly BloomConfiguration _configuration;
    private readonly ClientWebSocket _webSocketClient;
    private readonly HttpClient _restClient;
    private readonly Dictionary<ulong, BloomPlayer> _players;

    private string _sessionId;
    private Task? _receivingTask;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Occurs when an exception is thrown by the node.
    /// </summary>
    public event NodeExceptionEvent? NodeException;

    /// <summary>
    /// Occurs when a track starts playing.
    /// </summary>
    public event TrackStartEvent? TrackStarted;

    /// <summary>
    /// Occurs when a track ends playing.
    /// </summary>
    public event TrackEndEvent? TrackEnded;

    /// <summary>
    /// Occurs when an exception is thrown by a track.
    /// </summary>
    public event TrackExceptionEvent? TrackException;

    /// <summary>
    /// Occurs when a track gets stuck.
    /// </summary>
    public event TrackStuckEvent? TrackStucked;

    /// <summary>
    /// Gets a value indicating whether the node is connected to the remote server.
    /// </summary>
    public bool IsConnected => _webSocketClient.State is WebSocketState.Open;

    /// <summary>
    /// Initializes a new instance of the <see cref="BloomNode"/> class.
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="configuration">The configuration of the node.</param>
    public BloomNode(DiscordSocketClient discordClient, BloomConfiguration configuration)
    {
        _discordClient = discordClient;
        _configuration = configuration;
        _webSocketClient = new ClientWebSocket();
        _restClient = new HttpClient();
        _players = [];
        _sessionId = string.Empty;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (IsConnected)
            await DisconnectAsync();

        _webSocketClient.Dispose();
        _restClient.Dispose();
    }

    /// <summary>
    /// Returns the player of the specified guild.
    /// </summary>
    /// <param name="guild">The guild where the player is belong to.</param>
    /// <returns>The player that is found in the specified guild.</returns>
    public bool TryGetPlayer(IGuild guild, [MaybeNullWhen(false)] out BloomPlayer player)
    {
        return _players.TryGetValue(guild.Id, out player);
    }

    /// <summary>
    /// Connects the node to the remote server.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfConnected();

        _discordClient.VoiceServerUpdated += VoiceServerUpdated;
        _discordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;

        _webSocketClient.Options.SetRequestHeader("User-Id", _discordClient.CurrentUser.Id.ToString());
        _webSocketClient.Options.SetRequestHeader("Client-Name", $"{nameof(Bloom)}/{typeof(BloomNode).Assembly.GetName().Version}");
        _webSocketClient.Options.SetRequestHeader("Num-Shards", _configuration.ShardCount.ToString());
        _webSocketClient.Options.SetRequestHeader("Authorization", _configuration.Authorization);

        _restClient.BaseAddress = new Uri(_configuration.RestEndpoint);
        _restClient.DefaultRequestHeaders.Add("Authorization", _configuration.Authorization);

        async ValueTask EstablishConnectionAsync(int reconnectAttemps = 1)
        {
            if (reconnectAttemps > _configuration.ReconnectAttemps)
                throw new InvalidOperationException("The node could not establish a connection");

            try
            {
                await _webSocketClient.ConnectAsync(new Uri(_configuration.WebSocketEndpoint), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (WebSocketException ex)
            {
                NodeException?.Invoke(new NodeExceptionEventArgs(ex.Message));
                await Task.Delay(_configuration.ReconnectDelayInMiliseconds * reconnectAttemps, cancellationToken);
                await EstablishConnectionAsync(reconnectAttemps + 1);
            }
        }

        await EstablishConnectionAsync();

        _cancellationTokenSource = new CancellationTokenSource();
        _receivingTask = Task.Run(StartReceivingAsync, cancellationToken);
    }

    /// <summary>
    /// Disconnects the node from the remote server.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_receivingTask is not null)
            {
                await _receivingTask;
                _receivingTask = null;
            }

            await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect Request", cancellationToken);

            _discordClient.VoiceServerUpdated -= VoiceServerUpdated;
            _discordClient.UserVoiceStateUpdated -= UserVoiceStateUpdated;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            NodeException?.Invoke(new NodeExceptionEventArgs(ex.Message));
        }
    }

    /// <summary>
    /// Joins a new player to the specified voice channel.
    /// </summary>
    /// <param name="voiceChannel">The voice channel where the player will be connected to.</param>
    /// <param name="textChannel">The text channel where the player will be connected to.</param>
    /// <returns>The player that is connected to the voice channel.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask<BloomPlayer> JoinAsync(IVoiceChannel voiceChannel, IMessageChannel textChannel)
    {
        ThrowIfNotConnected();

        var player = new BloomPlayer(this, voiceChannel, textChannel);
        _players.Add(voiceChannel.GuildId, player);
        await player.ConnectAsync(_configuration.SelfDeaf, _configuration.SelfMute);
        return player;
    }

    /// <summary>
    /// Leaves the player from the specified guild.
    /// </summary>
    /// <param name="guild">The guild where the player will be disconnected from.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask LeaveAsync(IGuild guild)
    {
        ThrowIfNotConnected();

        if (!TryGetPlayer(guild, out BloomPlayer? player))
            throw new InvalidOperationException("The player is not found in the specified guild");

        await DeletePlayerAsync(player);
        await player.DisconnectAsync();
        _players.Remove(guild.Id);
    }

    /// <summary>
    /// Searches for tracks using the specified query and kind.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <param name="kind">The kind of the search.</param>
    /// <returns>A <see cref="LoadResult"/> as the result of the search.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async ValueTask<LoadResult> SearchAsync(string query, SearchKind kind, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        using HttpRequestMessage request = new(HttpMethod.Get, string.Format(LoadTracksEndpoint, kind.WrapQuery(query)));

        using HttpResponseMessage response = await _restClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            NodeException?.Invoke(new NodeExceptionEventArgs($"Failed to search for tracks: {content}"));
        }

        Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        JsonNode? node = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
        return ParseTool.ParseLoadResult(node);
    }

    internal async ValueTask UpdatePlayerAsync(BloomPlayer player, PlayerUpdatePayload updatePayload)
    {
        using HttpRequestMessage request = new(HttpMethod.Patch, string.Format(UpdatePlayerEndpoint, _sessionId, player.VoiceChannel.GuildId));
        request.Content = JsonContent.Create(updatePayload);

        using HttpResponseMessage response = await _restClient.SendAsync(request, _cancellationTokenSource!.Token);
        if (!response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            NodeException?.Invoke(new NodeExceptionEventArgs($"Failed to update player: {content}"));
        }
    }

    internal async ValueTask DeletePlayerAsync(BloomPlayer player)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, string.Format(UpdatePlayerEndpoint, _sessionId, player.VoiceChannel.GuildId));

        using HttpResponseMessage response = await _restClient.SendAsync(request, _cancellationTokenSource!.Token);
        if (!response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            NodeException?.Invoke(new NodeExceptionEventArgs($"Failed to delete player: {content}"));
        }
    }

    private async Task StartReceivingAsync()
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
        while (IsConnected && !_cancellationTokenSource!.IsCancellationRequested)
        {
            try
            {
                int length = await ReceiveDataAsync(buffer);
                using var stream = new MemoryStream(buffer, 0, length);
                JsonNode? node = await JsonNode.ParseAsync(stream, cancellationToken: _cancellationTokenSource.Token);
                HandleReceivedData(node);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                NodeException?.Invoke(new NodeExceptionEventArgs(ex.Message));
            }
        }
    }

    private async ValueTask<int> ReceiveDataAsync(byte[] buffer)
    {
        int length = 0;

        while (IsConnected && !_cancellationTokenSource!.IsCancellationRequested)
        {
            WebSocketReceiveResult result = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer, length, buffer.Length), _cancellationTokenSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            length += result.Count;
            if (result.EndOfMessage)
                break;
        }

        return length;
    }

    private void HandleReceivedData(JsonNode? node)
    {
        if (node is null)
            return;

        string op = node["op"]!.GetValue<string>();
        if (op == "ready")
        {
            _sessionId = node["sessionId"]!.GetValue<string>();
        }
        else if (op == "playerUpdate")
        {
            bool connected = node["state"]!["connected"]!.GetValue<bool>();
            if (connected)
            {
                ulong guildId = ulong.Parse(node["guildId"]!.GetValue<string>());
                IGuild guild = _discordClient.GetGuild(guildId);
                if (!TryGetPlayer(guild, out BloomPlayer? player))
                    return;

                if (player.Track is not null)
                {
                    long position = node["state"]!["position"]!.GetValue<long>();
                    player.Track.Position = TimeSpan.FromMilliseconds(position);
                }
            }
        }
        else if (op == "event")
        {
            ulong guildId = ulong.Parse(node["guildId"]!.GetValue<string>());
            IGuild guild = _discordClient.GetGuild(guildId);
            if (!TryGetPlayer(guild, out BloomPlayer? player))
                return;

            string eventType = node["type"]!.GetValue<string>();
            if (eventType == nameof(TrackStartEvent))
            {
                TrackStarted?.Invoke(new TrackStartEventArgs(player));
            }
            else if (eventType == nameof(TrackEndEvent))
            {
                TrackEnded?.Invoke(new TrackEndEventArgs(player, Enum.Parse<TrackEndReason>(node["reason"]!.GetValue<string>(), ignoreCase: true)));
            }
            else if (eventType == nameof(TrackExceptionEvent))
            {
                TrackException?.Invoke(new TrackExceptionEventArgs(player, ParseTool.ParseException(node["exception"]!)));
            }
            else if (eventType == nameof(TrackStuckEvent))
            {
                TrackStucked?.Invoke(new TrackStuckEventArgs(player, TimeSpan.FromMilliseconds(node["thresholdMs"]!.GetValue<long>())));
            }
        }
    }

    private async Task VoiceServerUpdated(SocketVoiceServer voiceServer)
    {
        if (!TryGetPlayer(voiceServer.Guild.Value, out BloomPlayer? player))
            return;

        await UpdatePlayerAsync(player, new PlayerUpdatePayload
        {
            Voice = new VoiceStatePayload
            {
                Token = voiceServer.Token,
                Endpoint = voiceServer.Endpoint,
                SessionId = player.VoiceSessionId,
            }
        });
    }

    private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        SocketGuildUser guildUser = (user as SocketGuildUser)!;
        if (!TryGetPlayer(guildUser.Guild, out BloomPlayer? player))
            return;

        if (guildUser.Id == _discordClient.CurrentUser.Id)
        {
            if (after.VoiceChannel is null)
            {
                await LeaveAsync(before.VoiceChannel.Guild);
                return;
            }

            player.VoiceSessionId = after.VoiceSessionId;
            player.VoiceChannel = after.VoiceChannel;
        }
        else if (!guildUser.IsBot && before.VoiceChannel is not null && after.VoiceChannel is null)
        {
            if (guildUser.Guild.VoiceChannels.All(static (voiceChannel) => !voiceChannel.ConnectedUsers.Any(static (user) => !user.IsBot)))
                await LeaveAsync(before.VoiceChannel.Guild);
        }
    }

    private void ThrowIfConnected()
    {
        if (IsConnected)
            throw new InvalidOperationException("The node already connected to the remote server");
    }

    private void ThrowIfNotConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("The node is not connected to the remote server");
    }
}
