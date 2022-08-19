namespace Blossom.Modules;

[Name("Moderation Module")]
public class ModerationModule : ModuleBase
{
    public ModerationModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Command("kick"), Summary("Kicks a member from this guild"), RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickCommand([Summary("The user to kick from guild")] SocketGuildUser target, [Summary("The reason of the action"), Remainder] string reason = null)
    {
        int userHierarchy = (User as SocketGuildUser).Roles.Max(r => r.Position);
        int targetHierarchy = target.Roles.Max(r => r.Position);

        if (User.Id != Guild.OwnerId && userHierarchy <= targetHierarchy)
        {
            await ReplyAsync("You need to be at higher position than `target` to kick!");
            return;
        }

        await target.KickAsync(reason ?? "No reason provided");
        await ReplyAsync($"{target.Username} is kicked from {Guild.Name}");
    }

    [Command("ban"), Summary("Bans a member from this guild"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanCommand([Summary("The user to ban from guild")] SocketGuildUser target, [Summary("The reason of the action"), Remainder] string reason = null)
    {
        int userHierarchy = (User as SocketGuildUser).Roles.Max(r => r.Position);
        int targetHierarchy = target.Roles.Max(r => r.Position);

        if (User.Id != Guild.OwnerId && userHierarchy <= targetHierarchy)
        {
            await ReplyAsync("You need to be at higher position than `target` to ban!");
            return;
        }

        await target.BanAsync(0, reason ?? "No reason provided");
        await ReplyAsync($"{target.Username} is banned from {Guild.Name}");
    }

    [Command("unban"), Summary("Removes ban from a banned user for this guild"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task UnbanCommand([Summary("The user id to remove ban for guild")] ulong target, [Summary("The reason of the action"), Remainder] string reason = null)
    {
        RestBan ban = await Guild.GetBanAsync(target);

        if (ban == null)
        {
            await ReplyAsync("Could not found ban object for `target`!");
            return;
        }

        await Guild.RemoveBanAsync(ban.User, new RequestOptions { AuditLogReason = reason });
        await ReplyAsync($"{ban.User.Username}'s ban removed from {Guild.Name}");
    }

    [Command("clear"), Summary("Clears the channel"), RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task ClearCommand([Summary("The amount of messages to delete")] int count)
    {
        SocketTextChannel channel = Channel as SocketTextChannel;
        IEnumerable<IMessage> messages = await channel.GetMessagesAsync(Math.Min(100, count) + 1).FlattenAsync();
        await channel.DeleteMessagesAsync(messages);
        IMessage message = await ReplyAsync($"✅ {messages.Count() - 1} message(s) deleted");
        await Task.Delay(1000);
        await message.DeleteAsync();
    }
}
