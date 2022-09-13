namespace Blossom.Modules;

[EnabledInDm(isEnabled: false)]
public sealed class InformationModule : InteractionModuleBase
{
    public static readonly Color ShadeGreen = new(129, 183, 26);

    public InformationModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("commands", "Sends the command list")]
    public async Task CommandsCommand()
    {
        FieldBuilder[] fields = InteractionService.Modules.Select((module) => new FieldBuilder($"> {module.Name}", string.Join("\n", module.SlashCommands.Select((command) => $"`{command.Name}`: {command.Description}")))).ToArray();
        await RespondWithEmbedAsync($"{Client.CurrentUser.Mention}'s Commands", fields);
    }

    [SlashCommand("status", "Shows my current status")]
    public async Task StatusCommand()
    {
        System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        string latency = $"{((Client.Latency < 100) ? "🟢" : ((Client.Latency < 250) ? "🟡" : "🔴"))} {Client.Latency} MS";
        string discordNetVersion = System.Reflection.Assembly.GetAssembly(typeof(DiscordSocketClient)).GetName().Version.ToString(3);
        string ramUsage = $"{currentProcess.PrivateMemorySize64 / 1048576} MB";
        string cpuTime = $"{currentProcess.TotalProcessorTime.TotalMilliseconds} MS";

        await RespondWithEmbedAsync("Current Status",
            new FieldBuilder("Latency", latency),
            new FieldBuilder(".NET Version", $"{Environment.Version}"),
            new FieldBuilder("Discord.NET Version", discordNetVersion),
            new FieldBuilder("Bot Version", Configuration.Version),
            new FieldBuilder("RAM Usage", ramUsage),
            new FieldBuilder("CPU Time", cpuTime)
        );
    }

    [SlashCommand("guild", "Shows current guild's information")]
    public async Task GuildCommand()
    {
        string channels = $"📁 {Guild.CategoryChannels.Count}\n💬 {Guild.TextChannels.Count}\n🔊 {Guild.VoiceChannels.Count}\nTotal: {Guild.Channels.Count}";
        string members = $"🟢 {Guild.Users.Count((user) => user.Status == UserStatus.Online)}\n🟡 {Guild.Users.Count((user) => user.Status == UserStatus.Idle)}\n🔴 {Guild.Users.Count((user) => user.Status == UserStatus.DoNotDisturb)}\n⚫ {Guild.Users.Count((user) => user.Status == UserStatus.Offline)}\nTotal: {Guild.Users.Count}";

        await RespondWithEmbedAsync("Guild Information", Guild.Description, Guild.BannerUrl,
            new AuthorBuilder(Guild.Name, Guild.IconUrl),
            new FieldBuilder("Id", Guild.Id),
            new FieldBuilder("Owner", Guild.Owner.Mention),
            new FieldBuilder("Created At", Guild.CreatedAt),
            new FieldBuilder("Channels", channels),
            new FieldBuilder("Members", members),
            new FieldBuilder("Emotes", Guild.Emotes.Count),
            new FieldBuilder("System Channel", Guild.SystemChannel?.Mention ?? "No system channel"),
            new FieldBuilder("Rules Channel", Guild.RulesChannel?.Mention ?? "No rules channel"),
            new FieldBuilder("AFK Channel", Guild.AFKChannel?.Mention ?? "No AFK channel"),
            new FieldBuilder("AFK Timeout", Guild.AFKTimeout),
            new FieldBuilder("Premium Tier", Guild.PremiumTier)
        );
    }

    [SlashCommand("user", "Shows a user's information")]
    public async Task UserCommand([Summary(description: "User to show information")] SocketGuildUser user = null)
    {
        user ??= User as SocketGuildUser;

        string activities = (user.Activities.Count == 0) ? "Idling" : string.Join('\n', user.Activities.Select((activity) => $"`{activity.Type} {activity.Name}` {activity.Details}"));
        SocketRole topRole = user.Roles.OrderByDescending((role) => role.Position).First();

        await RespondWithEmbedAsync("User Information", null, null,
            new AuthorBuilder(user.DisplayName, user.GetAvatarUrl()),
            new FieldBuilder("Id", user.Id),
            new FieldBuilder("User", user),
            new FieldBuilder("Is Bot?", user.IsBot),
            new FieldBuilder("Status", user.Status),
            new FieldBuilder("Activities", activities),
            new FieldBuilder("Top Role", topRole.Mention),
            new FieldBuilder("Created At", user.CreatedAt),
            new FieldBuilder("Joined At", user.JoinedAt)
        );
    }

    [SlashCommand("spotify", "Shows a user's spotify status")]
    public async Task SpotifyCommand([Summary(description: "User to show information")] SocketGuildUser user = null)
    {
        user ??= User as SocketGuildUser;
        foreach (IActivity activity in user.Activities)
        {
            if (activity is SpotifyGame spotify)
            {
                string details = $"{user.Mention} is listening [{spotify.TrackTitle}]({spotify.TrackUrl}) from {spotify.AlbumTitle}\n```[ {spotify.Elapsed.Value:mm':'ss} / {spotify.Duration.Value:mm':'ss}]```";
                await RespondWithEmbedAsync("Listening Spotify", details, spotify.AlbumArtUrl, ShadeGreen);
                return;
            }
        }

        await RespondEphemeralAsync("This user isn't listening Spotify now!");
    }
}
