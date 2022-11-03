using System.Globalization;

namespace Blossom.Preconditions;

public sealed class EnsureVoiceStateAttribute : PreconditionAttribute
{
    private const string InvalidVoiceStateMessage = "Player isn't currently {0}!";

    private readonly PlayerState _playerState;

    public EnsureVoiceStateAttribute(PlayerState playerState)
    {
        _playerState = playerState;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        LavaPlayer? player = services.GetRequiredService<AudioService>().GetPlayer(context.Guild);
        return Task.FromResult(
            (player is not null && player.PlayerState != _playerState)
                ? PreconditionResult.FromError(string.Format(InvalidVoiceStateMessage, _playerState.ToString().ToLower()))
                : PreconditionResult.FromSuccess()
        );
    }
}
