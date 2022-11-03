namespace Blossom.AutocompleteHandlers;

public sealed class FilterAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string current = autocompleteInteraction.Data.Current.Value.ToString() ?? string.Empty;
        IEnumerable<AutocompleteResult> suggestions = AudioService.Filters.Where((filter) => filter.Key.Contains(current)).Select(static (filter) => new AutocompleteResult(filter.Key, filter.Key));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions.Take(16)));
    }
}
