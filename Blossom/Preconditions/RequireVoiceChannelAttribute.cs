namespace Blossom.Preconditions;

public sealed class RequireVoiceChannelAttribute : PreconditionAttribute
{
    public const string NoVoiceChannel = "You must be joined to a voice channel!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        return context.User is not IVoiceState
            ? Task.FromResult(PreconditionResult.FromError(NoVoiceChannel))
            : Task.FromResult(PreconditionResult.FromSuccess());
    }
}
