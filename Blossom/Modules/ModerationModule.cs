namespace Blossom.Modules;

[EnabledInDm(isEnabled: false)]
public sealed class ModerationModule : InteractionModuleBase
{
    public ModerationModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("kick", "Kicks a member from this guild"), RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickCommand([Summary(description: "The user to kick from guild")] SocketGuildUser target, [Summary(description: "The reason of the action")] string reason = null)
    {
        int userHierarchy = (User as SocketGuildUser).Roles.Max((role) => role.Position);
        int targetHierarchy = target.Roles.Max((role) => role.Position);

        if (User.Id != Guild.OwnerId && userHierarchy <= targetHierarchy)
        {
            await RespondEphemeralAsync("You need to be at higher position than `target` to kick!");
            return;
        }

        await target.KickAsync(reason ?? "No reason provided");
        await RespondAsync($"`{target.Username}` is kicked from `{Guild.Name}`.");
    }

    [SlashCommand("ban", "Bans a member from this guild"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanCommand([Summary(description: "The user to ban from guild")] SocketGuildUser target, [Summary(description: "The reason of the action")] string reason = null)
    {
        int userHierarchy = (User as SocketGuildUser).Roles.Max((role) => role.Position);
        int targetHierarchy = target.Roles.Max((role) => role.Position);

        if (User.Id != Guild.OwnerId && userHierarchy <= targetHierarchy)
        {
            await RespondEphemeralAsync("You need to be at higher position than `target` to ban!");
            return;
        }

        await target.BanAsync(0, reason ?? "No reason provided");
        await RespondAsync($"`{target.Username}` is banned from `{Guild.Name}`.");
    }

    [SlashCommand("unban", "Removes ban from a banned user for this guild"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task UnbanCommand([Summary(description: "The user id to remove ban for guild"), Autocomplete(typeof(BanAutocompleteHandler))] ulong target, [Summary(description: "The reason of the action")] string reason = null)
    {
        RestBan ban = await Guild.GetBanAsync(target);

        if (ban == null)
        {
            await RespondAsync("Couldn't found ban object for `target`!");
            return;
        }

        await Guild.RemoveBanAsync(ban.User, new RequestOptions { AuditLogReason = reason });
        await RespondAsync($"`{ban.User.Username}`'s ban removed from `{Guild.Name}`.");
    }

    [SlashCommand("clear", "Clears the channel"), RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task ClearCommand([Summary(description: "The amount of messages to delete"), MinValue(1), MaxValue(128)] int count)
    {
        SocketTextChannel channel = Channel as SocketTextChannel;
        IEnumerable<IMessage> messages = await channel.GetMessagesAsync(count).FlattenAsync();
        await channel.DeleteMessagesAsync(messages);
        await RespondEphemeralAsync($"✅ `{messages.Count()}` message(s) deleted.");
    }
}
