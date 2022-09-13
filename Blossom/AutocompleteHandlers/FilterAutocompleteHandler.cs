namespace Blossom.AutocompleteHandlers;

public sealed class FilterAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        IEnumerable<AutocompleteResult> suggestions = AudioService.Filters.Where((filter) => filter.Key.Contains(autocompleteInteraction.Data.Current.Value.ToString())).Select((filter) => new AutocompleteResult(filter.Key, filter.Key));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions.Take(16)));
    }
}
