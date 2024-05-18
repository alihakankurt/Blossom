using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Blossom.Preconditions;

public sealed class RequireVoiceChannelAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        IVoiceState voiceState = (IVoiceState)context.User;

        if (voiceState.VoiceChannel is null)
        {
            return Task.FromResult(PreconditionResult.FromError("You must be joined to a voice channel!"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
