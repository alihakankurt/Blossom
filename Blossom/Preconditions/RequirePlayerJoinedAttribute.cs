namespace Blossom.Preconditions;

public sealed class RequirePlayerJoinedAttribute : PreconditionAttribute
{
    private const string NotJoinedMessage = "I'm not joined to voice a channel!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        return Task.FromResult(
            !services.GetRequiredService<AudioService>().IsJoined(context.Guild)
                ? PreconditionResult.FromError(NotJoinedMessage)
                : PreconditionResult.FromSuccess()
        );
    }
}
