namespace Bloom;

public sealed class BloomConfig
{
    public static BloomConfig Default => new();

    public string Hostname { get; init; } = "127.0.0.1";
    public ushort Port { get; init; } = 2333;
    public string Authorization { get; init; } = "youshallnotpass";
    public bool IsSecure { get; init; } = false;
    public int ShardCount { get; init; } = 1;
    public bool SelfDeaf { get; init; } = true;
    public bool SelfMute { get; init; } = false;
    public int ReconnectAttemps { get; init; } = 10;
    public int ReconnectDelayInMiliseconds { get; init; } = 10 * 1000;
    public int LeaveDelayInMiliseconds { get; init; } = 2 * 60 * 1000;

    internal string Endpoint => $"{(IsSecure ? "wss" : "ws")}://{Hostname}:{Port}";
    internal string RestEndpoint => $"{(IsSecure ? "https" : "http")}://{Hostname}:{Port}";
}
