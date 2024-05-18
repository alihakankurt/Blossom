namespace Bloom;

/// <summary>
/// Represents the configuration of the <see cref="BloomNode"/>.
/// </summary>
public sealed class BloomConfiguration
{
    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    public static BloomConfiguration Default => new();

    /// <summary>
    /// The hostname of the Lavalink server.
    /// </summary>
    public string Hostname { get; init; } = "127.0.0.1";

    /// <summary>
    /// The port of the Lavalink server.
    /// </summary>
    public ushort Port { get; init; } = 2333;

    /// <summary>
    /// The password of the Lavalink server.
    /// </summary>
    public string Authorization { get; init; } = "youshallnotpass";

    /// <summary>
    /// Whether the connection is secure.
    /// </summary>
    public bool IsSecure { get; init; } = false;

    /// <summary>
    /// The number of shards to use.
    /// </summary>
    public int ShardCount { get; init; } = 1;

    /// <summary>
    /// Whether the bot should deafen itself.
    /// </summary>
    public bool SelfDeaf { get; init; } = true;

    /// <summary>
    /// Whether the bot should mute itself.
    /// </summary>
    public bool SelfMute { get; init; } = false;

    /// <summary>
    /// The number of reconnect attempts.
    /// </summary>
    public int ReconnectAttemps { get; init; } = 10;

    /// <summary>
    /// The delay in milliseconds between each reconnect attempt.
    /// </summary>
    public int ReconnectDelayInMiliseconds { get; init; } = 10_000;

    internal string WebSocketEndpoint => $"{(IsSecure ? "wss" : "ws")}://{Hostname}:{Port}/v4/websocket";
    internal string RestEndpoint => $"{(IsSecure ? "https" : "http")}://{Hostname}:{Port}/v4/";
}
