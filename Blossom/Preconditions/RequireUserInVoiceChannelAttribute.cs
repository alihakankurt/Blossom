namespace Blossom.Preconditions;

public sealed class RequireUserInVoiceChannelAttribute : PreconditionAttribute
{
    public const string UserNotInVoiceChannel = "You must be joined to {0}";

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        LavaPlayer player = AudioService.GetPlayer(context.Guild);
        return player is not null && (context.User is not IVoiceState voiceState || player.VoiceChannel != voiceState.VoiceChannel)
            ? Task.FromResult(PreconditionResult.FromError(string.Format(UserNotInVoiceChannel, player.VoiceChannel.Mention)))
            : Task.FromResult(PreconditionResult.FromSuccess());
    }
}
