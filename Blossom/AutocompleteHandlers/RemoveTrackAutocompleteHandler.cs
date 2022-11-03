namespace Blossom.AutocompleteHandlers;

public sealed class RemoveTrackAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        LavaPlayer? player = services.GetRequiredService<AudioService>().GetPlayer(context.Guild);
        IEnumerable<AutocompleteResult>? suggestions = player?.Vueue.Take(16).Select(static (track, index) => new AutocompleteResult(track.Title, index + 1));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}
