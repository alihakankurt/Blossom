namespace Blossom.Preconditions;

public sealed class RequireVoiceChannelAttribute : PreconditionAttribute
{
    private const string NoVoiceChannel = "You must be joined to a voice channel!";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        return Task.FromResult(
            (((IVoiceState)context.User).VoiceChannel is null)
                ? PreconditionResult.FromError(NoVoiceChannel)
                : PreconditionResult.FromSuccess()
        );
    }
}
