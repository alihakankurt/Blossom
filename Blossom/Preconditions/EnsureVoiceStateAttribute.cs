namespace Blossom.Preconditions;

public sealed class EnsureVoiceStateAttribute : PreconditionAttribute
{
    public const string InvalidVoiceStateMessage = "Player isn't currently {0}!";

    private readonly PlayerState _playerState;

    public EnsureVoiceStateAttribute(PlayerState playerState)
    {
        _playerState = playerState;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        LavaPlayer player = AudioService.GetPlayer(context.Guild);
        return player is not null && player.PlayerState != _playerState
            ? Task.FromResult(PreconditionResult.FromError(string.Format(InvalidVoiceStateMessage, _playerState.ToString().ToLower())))
            : Task.FromResult(PreconditionResult.FromSuccess());
    }
}
