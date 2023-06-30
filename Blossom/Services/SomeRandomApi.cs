namespace Blossom.Services;

public static class SomeRandomApi
{
    private const string Endpoint = "https://some-random-api.com";

    public static async ValueTask<string?> GetLyricsAsync(string title)
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{Endpoint}/others/lyrics?title={title}");
        string? response = await requestMessage.SendAsync();
        if (response is null)
            return null;

        JsonObject data = JsonNode.Parse(response)!.AsObject();
        return data["lyrics"]!.ToString();
    }

    public static async ValueTask<string?> GetJokeAsync()
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{Endpoint}/others/joke");
        string? response = await requestMessage.SendAsync();
        if (response is null)
            return null;

        JsonObject data = JsonNode.Parse(response)!.AsObject();
        return data["joke"]!.ToString();
    }

    public static async ValueTask<string?> GetQuoteAsync()
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Get, $"{Endpoint}/animu/quote");
        string? response = await requestMessage.SendAsync();
        if (response is null)
            return null;

        JsonObject data = JsonNode.Parse(response)!.AsObject();
        return $"{data["sentence"]}\n\t- {data["character"]}, {data["anime"]}";
    }
}
