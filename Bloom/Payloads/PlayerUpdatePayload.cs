namespace Bloom.Payloads;

internal sealed class PlayerUpdatePayload : IPayload
{
    [JsonPropertyName("encodedTrack"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EncodedTrack { get; set; }

    [JsonPropertyName("identifier"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Identifier { get; set; }

    [JsonPropertyName("position"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long? Position { get; set; }

    [JsonPropertyName("endTime"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long? EndTime { get; set; }

    [JsonPropertyName("volume"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? Volume { get; set; }

    [JsonPropertyName("paused"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? Paused { get; set; }

    [JsonPropertyName("filters"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public FilterPayload? Filters { get; set; }

    [JsonPropertyName("voice"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public VoiceStatePayload? Voice { get; set; }
}
