using Bloom.Filters;
using Discord;
using Discord.Interactions;

namespace Blossom.AutoCompleteHandlers;

public sealed class FilterAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string current = autocompleteInteraction.Data.Current.Value.ToString() ?? string.Empty;
        IEnumerable<AutocompleteResult> suggestions = FilterPreset.Presets
            .Where((filter) => filter.Name.Contains(current, StringComparison.InvariantCultureIgnoreCase))
            .Take(12)
            .Select(static (filter) => new AutocompleteResult(filter.Name, filter.Name));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}
