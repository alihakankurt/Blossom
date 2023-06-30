namespace Bloom.Payloads;

internal sealed class VoiceStatePayload : IPayload
{
    [JsonPropertyName("token"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Token { get; set; }

    [JsonPropertyName("endpoint"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Endpoint { get; set; }

    [JsonPropertyName("sessionId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SessionId { get; set; }

    public VoiceStatePayload(string? token, string? endpoint, string? sessionId)
    {
        Token = token;
        Endpoint = endpoint;
        SessionId = sessionId;
    }
}
