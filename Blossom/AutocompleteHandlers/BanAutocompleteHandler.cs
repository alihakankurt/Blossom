﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Blossom.AutoCompleteHandlers;

public sealed class BanAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string current = autocompleteInteraction.Data.Current.Value.ToString() ?? string.Empty;
        IEnumerable<IBan> bans = await context.Guild.GetBansAsync().FlattenAsync();
        IEnumerable<AutocompleteResult> suggestions = bans.Where((ban) => ban.User.Username.Contains(current))
            .Select(static (ban) => new AutocompleteResult(ban.User.Username, ban.User.Id));
        return AutocompletionResult.FromSuccess(suggestions.Take(16));
    }
}
