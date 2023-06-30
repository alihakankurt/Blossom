namespace Blossom.Preconditions;

public sealed class RequirePlayerNotJoinedAttribute : PreconditionAttribute
{
    private const string AlreadyJoinedMessage = "I'm already joined to voice a channel!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        return Task.FromResult(
            services.GetRequiredService<BloomNode>().HasPlayer(context.Guild)
                ? PreconditionResult.FromError(AlreadyJoinedMessage)
                : PreconditionResult.FromSuccess()
        );
    }
}
