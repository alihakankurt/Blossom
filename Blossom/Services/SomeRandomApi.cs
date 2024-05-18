using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blossom.Services;

public sealed class SomeRandomApi : IService
{
    private const string EndPoint = "https://some-random-api.com/";

    private readonly HttpClient _httpClient;

    public SomeRandomApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(EndPoint);
    }

    public ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask<LyricsResult?> GetLyricsAsync(string title)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync($"others/lyrics?title={title}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LyricsResult>();
    }

    public async ValueTask<AnimeQuote?> GetAnimeQuoteAsync()
    {
        using HttpResponseMessage response = await _httpClient.GetAsync("anime/quote");
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
