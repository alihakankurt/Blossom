namespace Blossom.Preconditions;

public sealed class RequireUserInVoiceChannelAttribute : PreconditionAttribute
{
    private const string UserNotInVoiceChannel = "You must be joined to {0}";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        LavaPlayer? player = services.GetRequiredService<AudioService>().GetPlayer(context.Guild);
        return Task.FromResult(
            (player is not null && player.VoiceChannel != ((IVoiceState)context.User).VoiceChannel)
                ? PreconditionResult.FromError(string.Format(UserNotInVoiceChannel, player.VoiceChannel.Mention))
                : PreconditionResult.FromSuccess()
        );
    }
}
