namespace Blossom.AutoCompleteHandlers;

public sealed class RemoveTrackAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string current = autocompleteInteraction.Data.Current.Value.ToString()?.ToLowerInvariant() ?? string.Empty;
        BloomPlayer? player = services.GetRequiredService<BloomNode>().GetPlayer(context.Guild);
        IEnumerable<AutocompleteResult>? suggestions = player?.Queue
            .Where((track, index) => track is not null
                && index != player.Queue.Current && index < 25
                && track.Title.ToLowerInvariant().Contains(current)
            )
            .Select((track, index) => new AutocompleteResult(track.Title, index + (player.Queue.Current <= index ? 2 : 1)));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}
