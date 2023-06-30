namespace Blossom.Preconditions;

public sealed class EnsurePlayerStateAttribute : PreconditionAttribute
{
    private const string InvalidPlayerStateMessage = "Player isn't currently {0}!";

    private readonly PlayerState _playerState;

    public EnsurePlayerStateAttribute(PlayerState playerState)
    {
        _playerState = playerState;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        BloomPlayer? player = services.GetRequiredService<BloomNode>().GetPlayer(context.Guild);
        return Task.FromResult(
            (player is not null && player.State != _playerState)
                ? PreconditionResult.FromError(string.Format(InvalidPlayerStateMessage, _playerState.ToString().ToLowerInvariant()))
                : PreconditionResult.FromSuccess()
        );
    }
}
