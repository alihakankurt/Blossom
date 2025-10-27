using System.Net.Http.Json;

namespace Blossom.Services;

public sealed class SomeRandomApi
{
    private const string BaseAddress = "https://some-random-api.com";

    private readonly HttpClient _httpClient;

    public SomeRandomApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<LyricsResult?> GetLyricsAsync(string title)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync($"{BaseAddress}/others/lyrics?title={title}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LyricsResult>();
    }

    public async ValueTask<AnimeQuote?> GetAnimeQuoteAsync()
    {
        using HttpResponseMessage response = await _httpClient.GetAsync($"{BaseAddress}/anime/quote");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AnimeQuote>();
    }
}

public sealed class LyricsResult
{
    public string Title { get; } = string.Empty;
    public string Author { get; } = string.Empty;
    public string Lyrics { get; } = string.Empty;
    public string Thumbnail { get; } = string.Empty;
}

public sealed class AnimeQuote
{
    public string Sentence { get; } = string.Empty;
    public string Character { get; } = string.Empty;
    public string Anime { get; } = string.Empty;
}
