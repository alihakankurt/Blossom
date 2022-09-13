namespace Blossom.Preconditions;

public sealed class RequirePlayerJoinedAttribute : PreconditionAttribute
{
    public const string NotJoinedMessage = "I'm not joined to voice a channel!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        LavaPlayer player = AudioService.GetPlayer(context.Guild);
        return player == null
            ? Task.FromResult(PreconditionResult.FromError(NotJoinedMessage))
            : Task.FromResult(PreconditionResult.FromSuccess());
    }
}
