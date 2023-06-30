namespace Blossom.Preconditions;

public sealed class EnsureNotLoopModeAttribute : PreconditionAttribute
{
    private const string InvalidLoopModeMessage = "This can't be used when loop mode is set to: `{0}` !";

    private readonly LoopMode _loopMode;

    public EnsureNotLoopModeAttribute(LoopMode loopMode)
    {
        _loopMode = loopMode;
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        BloomPlayer? player = services.GetRequiredService<BloomNode>().GetPlayer(context.Guild);
        return Task.FromResult(
            (player is not null && player.Queue.LoopMode == _loopMode)
                ? PreconditionResult.FromError(string.Format(InvalidLoopModeMessage, _loopMode.ToString()))
                : PreconditionResult.FromSuccess()
        );
    }
}
