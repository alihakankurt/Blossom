using Bloom.Playback;
using Blossom.Services;
using Blossom.Utilities;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom.AutoCompleteHandlers;

public sealed class RemoveTrackAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string current = autocompleteInteraction.Data.Current.Value.ToString() ?? string.Empty;
        BloomPlayer? player = services.GetRequiredService<AudioService>().GetPlayer(context.Guild);
        if (player is null)
        {
            return Task.FromResult(AutocompletionResult.FromSuccess());
        }

        int trackIndex = 0;
        List<AutocompleteResult> suggestions = [];
        foreach (BloomTrack track in player.Queue.Tracks)
        {
            trackIndex += 1;

            if (track == player.Queue.CurrentTrack)
            {
                continue;
            }

            if (track.Title.Contains(current, StringComparison.InvariantCultureIgnoreCase))
            {
                suggestions.Add(new AutocompleteResult(track.Title.EndAt(100), trackIndex));
                if (suggestions.Count > 12)
                {
                    break;
                }
            }
        }

        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}
