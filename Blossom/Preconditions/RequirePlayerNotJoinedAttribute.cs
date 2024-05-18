using System;
using System.Threading.Tasks;
using Blossom.Services;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom.Preconditions;

public sealed class RequirePlayerNotJoinedAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        AudioService audioService = services.GetRequiredService<AudioService>();

        if (audioService.HasPlayer(context.Guild))
        {
            return Task.FromResult(PreconditionResult.FromError("I'm already joined to voice a channel!"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
