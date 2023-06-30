namespace Blossom.Preconditions;

public sealed class RequireNonEmptyQueueAttribute : PreconditionAttribute
{
    private const string QueueIsEmptyMessage = "The queue of the player is empty!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        BloomPlayer? player = services.GetRequiredService<BloomNode>().GetPlayer(context.Guild);
        return Task.FromResult(
            (player is not null && player.Queue.IsEmpty)
                ? PreconditionResult.FromError(QueueIsEmptyMessage)
                : PreconditionResult.FromSuccess()
        );
    }
}
