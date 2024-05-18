using System;
using System.Threading.Tasks;
using Bloom.Playback;
using Blossom.Services;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom.Preconditions;

public sealed class RequirePlayerStateAttribute : PreconditionAttribute
{
    private readonly PlayerState _playerState;

    public RequirePlayerStateAttribute(PlayerState playerState)
    {
        _playerState = playerState;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        AudioService audioService = services.GetRequiredService<AudioService>();
        BloomPlayer? player = audioService.GetPlayer(context.Guild);

        if (player?.State != _playerState)
        {
            return Task.FromResult(PreconditionResult.FromError($"Player isn't currently {_playerState:G}!"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
