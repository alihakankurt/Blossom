namespace Blossom.Modules;

public sealed class ModerationModule : BaseInteractionModule
{
    private const string NoReasonProvided = "No reason provided";

    public ModerationModule(IServiceProvider services) : base(services)
    {
    }

    [SlashCommand("kick", "Kicks a member from this guild"), RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickCommand([Summary(description: "The user to kick from guild")] SocketGuildUser target, [Summary(description: "The reason of the action")] string? reason = default)
    {
        int userHierarchy = ((SocketGuildUser)User).GetTopRole().Position;
        int targetHierarchy = target.GetTopRole().Position;

        if (User.Id != Guild.OwnerId && userHierarchy <= targetHierarchy)
        {
            await RespondEphemeralAsync("You need to be at higher position than `target` to kick!");
            return;
        }

        await target.KickAsync(reason ?? NoReasonProvided);
        await RespondAsync($"`{target.Username}` is kicked from `{Guild.Name}`.");
    }

    [SlashCommand("ban", "Bans a member from this guild"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanCommand([Summary(description: "The user to ban from guild")] SocketGuildUser target, [Summary(description: "The reason of the action")] string? reason = default)
    {
        int userHierarchy = ((SocketGuildUser)User).GetTopRole().Position;
        int targetHierarchy = target.GetTopRole().Position;

        if (User.Id != Guild.OwnerId && userHierarchy <= targetHierarchy)
        {
            await RespondEphemeralAsync("You need to be at higher position than `target` to ban!");
            return;
        }

        await target.BanAsync(0, reason ?? NoReasonProvided);
        await RespondAsync($"`{target.Username}` is banned from `{Guild.Name}`.");
    }

    [SlashCommand("unban", "Removes ban from a banned user for this guild"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task UnbanCommand([Summary(description: "The user id to remove ban for guild"), AutoComplete<BanAutoCompleteHandler>()] ulong target, [Summary(description: "The reason of the action")] string? reason = default)
    {
        RestBan ban = await Guild.GetBanAsync(target);

        if (ban is null)
        {
            await RespondAsync("Couldn't found ban object for `target`!");
            return;
        }

        await Guild.RemoveBanAsync(ban.User, new RequestOptions { AuditLogReason = reason ?? NoReasonProvided });
        await RespondAsync($"`{ban.User.Username}`'s ban removed from `{Guild.Name}`.");
    }

    [SlashCommand("clear", "Clears the channel"), RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task ClearCommand([Summary(description: "The amount of messages to delete"), MinValue(1), MaxValue(128)] int count)
    {
        SocketTextChannel channel = (SocketTextChannel)Channel;
        IEnumerable<IMessage> messages = await channel.GetMessagesAsync(count).FlattenAsync();
        await channel.DeleteMessagesAsync(messages);
        await RespondEphemeralAsync($"✅ `{messages.Count()}` message(s) deleted.");
    }
}