using System;
using System.Threading.Tasks;
using Bloom.Playback;
using Blossom.Services;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom.Preconditions;

public sealed class RequireUserInVoiceChannelAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        AudioService audioService = services.GetRequiredService<AudioService>();
        BloomPlayer? player = audioService.GetPlayer(context.Guild);

        if (player is not null && player.VoiceChannel != ((IVoiceState)context.User).VoiceChannel)
        {
            return Task.FromResult(PreconditionResult.FromError($"You must be joined to {player.VoiceChannel.Mention}"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
