using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        string current = autocompleteInteraction.Data.Current.Value.ToString()?.ToLowerInvariant() ?? string.Empty;
        BloomPlayer? player = services.GetRequiredService<AudioService>().GetPlayer(context.Guild);
        IEnumerable<AutocompleteResult>? suggestions = player?.Queue
            .Where((track, index) => track is not null
                && index != player.Queue.Current && index < 25
                && track.Title.Contains(current, StringComparison.InvariantCultureIgnoreCase)
            )
            .Select((track, index) => new AutocompleteResult(track.Title.EndAt(100), index + (player.Queue.Current <= index ? 2 : 1)));
        return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
    }
}
