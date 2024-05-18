using System;
using System.Threading.Tasks;
using Bloom.Playback;
using Blossom.Services;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom.Preconditions;

public sealed class RequireNonEmptyQueueAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        AudioService audioService = services.GetRequiredService<AudioService>();
        BloomPlayer? player = audioService.GetPlayer(context.Guild);

        if (player is not null && player.Queue.Count == 0)
        {
            return Task.FromResult(PreconditionResult.FromError("The queue of the player is empty!"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
