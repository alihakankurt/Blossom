namespace Blossom.AutocompleteHandlers;

public sealed class TrackAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        LavaPlayer player = AudioService.GetPlayer(context.Guild);
        IEnumerable<AutocompleteResult> suggestions = player?.Queue.Take(16).Select((track, index) => new AutocompleteResult(track.Title, index + 1));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}
