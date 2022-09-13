namespace Blossom.Preconditions;

public sealed class RequirePlayerNotJoinedAttribute : PreconditionAttribute
{
    public const string AlreadyJoinedMessage = "I'm already joined to voice a channel!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        LavaPlayer player = AudioService.GetPlayer(context.Guild);
        return player != null
            ? Task.FromResult(PreconditionResult.FromError(AlreadyJoinedMessage))
            : Task.FromResult(PreconditionResult.FromSuccess());
    }
}
