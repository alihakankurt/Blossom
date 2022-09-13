namespace Blossom.AutocompleteHandlers;

public sealed class BanAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        IEnumerable<IBan> bans = await context.Guild.GetBansAsync().FlattenAsync();
        IEnumerable<AutocompleteResult> suggestions = bans.Where((ban) => ban.User.Username.Contains(autocompleteInteraction.Data.Current.Value.ToString())).Select((ban) => new AutocompleteResult(ban.User.Username, ban.User.Id));
        return AutocompletionResult.FromSuccess(suggestions.Take(16));
    }
}
