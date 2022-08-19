namespace Blossom.Modules;

[Name("Information Module")]
public class InformationModule : ModuleBase
{
    public InformationModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Command("status"), Summary("Shows my current status.")]
    public async Task StatusCommand()
    {
        System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        await ReplyWithEmbedAsync("Current Status",
            new FieldBuilder("Latency", $"{((Client.Latency < 100) ? "🟢" : ((Client.Latency < 250) ? "🟡" : "🔴"))} {Client.Latency} MS"),
            new FieldBuilder(".NET Version", $"{Environment.Version}"),
            new FieldBuilder("Discord.NET Version", System.Reflection.Assembly.GetAssembly(typeof(DiscordSocketClient)).GetName().Version.ToString(3)),
            new FieldBuilder("Bot Version", Configuration.Version),
            new FieldBuilder("RAM Usage", $"{currentProcess.PrivateMemorySize64 / 1048576} MB"),
            new FieldBuilder("CPU Time", $"{currentProcess.TotalProcessorTime.TotalMilliseconds} MS")
        );
    }

    [Command("guild"), Summary("Shows current guild's information")]
    public async Task GuildCommand()
    {
        await ReplyWithEmbedAsync("Guild Information", Guild.Description, Guild.BannerUrl,
            new AuthorBuilder(Guild.Name, Guild.IconUrl),
            new FieldBuilder("Id", Guild.Id),
            new FieldBuilder("Owner", Guild.Owner.Mention),
            new FieldBuilder("Created At", Guild.CreatedAt),
            new FieldBuilder("Channels", $"📁 {Guild.CategoryChannels.Count}\n💬 {Guild.TextChannels.Count}\n🔊 {Guild.VoiceChannels.Count}\nTotal: {Guild.Channels.Count}"),
            new FieldBuilder("Members", $"🟢 {Guild.Users.Count(u => u.Status == UserStatus.Online)}\n🟡 {Guild.Users.Count(u => u.Status == UserStatus.Idle)}\n🔴 {Guild.Users.Count(u => u.Status == UserStatus.DoNotDisturb)}\n⚫ {Guild.Users.Count(u => u.Status == UserStatus.Offline)}\nTotal: {Guild.Users.Count}"),
            new FieldBuilder("Emotes", Guild.Emotes.Count),
            new FieldBuilder("System Channel", Guild.SystemChannel?.Mention ?? "No system channel"),
            new FieldBuilder("Rules Channel", Guild.RulesChannel?.Mention ?? "No rules channel"),
            new FieldBuilder("AFK Channel", Guild.AFKChannel?.Mention ?? "No AFK channel"),
            new FieldBuilder("AFK Timeout", Guild.AFKTimeout),
            new FieldBuilder("Premium Tier", Guild.PremiumTier)
        );
    }

    [Command("user"), Summary("Shows a user's information")]
    public async Task UserCommand([Summary("User to show information")] SocketGuildUser user = null)
    {
        user ??= User as SocketGuildUser;
        await ReplyWithEmbedAsync("User Information", null, null,
            new AuthorBuilder(user.DisplayName, user.GetAvatarUrl()),
            new FieldBuilder("Id", user.Id),
            new FieldBuilder("User", user),
            new FieldBuilder("Is Bot?", user.IsBot),
            new FieldBuilder("Status", user.Status),
            new FieldBuilder("Activities", (user.Activities.Count == 0) ? "Idling" : string.Join('\n', user.Activities.Select(a => $"`{a.Type} {a.Name}` {a.Details}"))),
            new FieldBuilder("Top Role", user.Roles.OrderByDescending(r => r.Position).First().Mention),
            new FieldBuilder("Created At", user.CreatedAt),
            new FieldBuilder("Joined At", user.JoinedAt)
        );
    }

    [Command("spotify"), Summary("Shows a user's spotify status")]
    public async Task SpotifyCommand([Summary("User to show information")] SocketGuildUser user = null)
    {
        user ??= User as SocketGuildUser;
        foreach (IActivity activity in user.Activities)
        {
            if (activity is SpotifyGame spotify)
            {
                await ReplyWithEmbedAsync("Listening Spotify", $"{user.Mention} is listening [{spotify.TrackTitle}]({spotify.TrackUrl}) from {spotify.AlbumTitle}\n```[ {spotify.Elapsed.Value:mm':'ss} / {spotify.Duration.Value:mm':'ss}]```", spotify.AlbumArtUrl);
                return;
            }
        }

        await ReplyAsync("This user is not listening Spotify now!");
    }
}
