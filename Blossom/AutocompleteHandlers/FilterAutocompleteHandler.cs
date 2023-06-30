namespace Blossom.AutoCompleteHandlers;

public sealed class FilterAutoCompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string current = autocompleteInteraction.Data.Current.Value.ToString()?.ToLowerInvariant() ?? string.Empty;
        IEnumerable<AutocompleteResult> suggestions = FilterPreset.Presets
            .Where((filter) => filter.Name.ToLowerInvariant().Contains(current))
            .Select(static (filter) => new AutocompleteResult(filter.Name, filter.Name));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions.Take(16)));
    }
}
