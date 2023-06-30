namespace Blossom.Modules;

public sealed class InformationModule : BaseInteractionModule
{
    private const string GreenCircle = ":green_circle:";
    private const string YelloCircle = ":yellow_circle:";
    private const string RedCircle = ":red_circle:";
    private const string BlackCircle = ":black_circle:";
    private const string NoSystemChannel = "No System Channel";
    private const string NoRulesChannel = "No Rules Channel";
    private const string NoAFKChannel = "No AFK Channel";
    private const string NoActivity = "Idling";

    public InformationModule(IServiceProvider services) : base(services)
    {
    }

    [SlashCommand("commands", "Shows the command list")]
    public async Task CommandsCommand()
    {
        await RespondWithEmbedAsync(
            description: $"{Client.CurrentUser.Mention}'s Commands",
            fields: InteractionService.Modules
                .Select(static (module) => CreateField(
                    $"> {module.Name}",
                    string.Join("\n", module.SlashCommands.Select(static (command) => $"`{command.Name}`: {command.Description}"))
                )
            )
                .ToArray()
        );
    }

    [SlashCommand("status", "Shows my current status")]
    public async Task StatusCommand()
    {
        string dotNetVersion = Environment.Version.ToString(3);
        string discordNetVersion = typeof(DiscordSocketClient).Assembly.GetName().Version!.ToString(3);
        string botVersion = ConfigurationService.Get("Version");

        string latency = $"{((Client.Latency < 100) ? GreenCircle : (Client.Latency < 250) ? YelloCircle : RedCircle)} {Client.Latency} MS";
        Process currentProcess = Process.GetCurrentProcess();
        string ramUsage = $"{currentProcess.PrivateMemorySize64 / 1048576} MB";
        string cpuTime = $"{currentProcess.TotalProcessorTime.TotalMilliseconds} MS";

        await RespondWithEmbedAsync(
            description: "Current Status",
            fields: new[]
            {
                CreateField(".NET Version", dotNetVersion),
                CreateField("Discord.NET Version", discordNetVersion),
                CreateField("Bot Version", botVersion),
                CreateField("Latency", latency),
                CreateField("RAM Usage", ramUsage),
                CreateField("CPU Time", cpuTime),
            }
        );
    }

    [SlashCommand("guild", "Shows current guild's information")]
    public async Task GuildCommand()
    {
        int categoryChannels = Guild.CategoryChannels.Count;
        int textChannels = Guild.TextChannels.Count;
        int voiceChannels = Guild.VoiceChannels.Count;
        int stageChannels = Guild.StageChannels.Count;
        int threadChannels = Guild.ThreadChannels.Count;

        int onlineMembers = Guild.Users.Count(static (user) => user.Status is UserStatus.Online);
        int idleMembers = Guild.Users.Count(static (user) => user.Status is UserStatus.Idle);
        int dndMembers = Guild.Users.Count(static (user) => user.Status is UserStatus.DoNotDisturb);
        int offlineMembers = Guild.Users.Count(static (user) => user.Status is UserStatus.Offline);

        await RespondWithEmbedAsync(
            title: "Guild Information",
            description: Guild.Description,
            thumbnail: Guild.BannerUrl,
            author: CreateAuthor(Guild.Name, Guild.IconUrl),
            fields: new[]
            {
                CreateField("Id", Guild.Id),
                CreateField("Owner", Guild.Owner.Mention),
                CreateField("Created At", Guild.CreatedAt),
                CreateField("Channels", $"📁 {categoryChannels}\n💬 {textChannels}\n🔊 {voiceChannels}\n🎙️ {stageChannels}\n🧵 {threadChannels}"),
                CreateField("Members", $"{GreenCircle} {onlineMembers}\n{YelloCircle} {idleMembers}\n{RedCircle} {dndMembers}\n{BlackCircle} {offlineMembers}"),
                CreateField("Emotes", Guild.Emotes.Count),
                CreateField("System Channel", Guild.SystemChannel?.Mention ?? NoSystemChannel),
                CreateField("Rules Channel", Guild.RulesChannel?.Mention ?? NoRulesChannel),
                CreateField("AFK Channel", Guild.AFKChannel?.Mention ?? NoAFKChannel),
                CreateField("AFK Timeout", Guild.AFKTimeout),
                CreateField("Premium Tier", Guild.PremiumTier),
            }
        );
    }

    [SlashCommand("user", "Shows a user's information")]
    public async Task UserCommand([Summary(description: "User to show information")] SocketGuildUser? user = null)
    {
        user ??= (SocketGuildUser)User;

        string activities = (user.Activities.Count is 0)
            ? NoActivity
            : string.Join('\n', user.Activities.Select(static (activity) => $"`{activity.Type} {activity.Name}` {activity.Details}"));

        SocketRole topRole = user.GetTopRole();

        await RespondWithEmbedAsync(
            title: "User Information",
            author: CreateAuthor(user.DisplayName, user.GetAvatarUrl()),
            fields: new[]
            {
                CreateField("Id", user.Id),
                CreateField("Username", user.Username),
                CreateField("Display Name", user.GlobalName),
                CreateField("Is Bot?", user.IsBot),
                CreateField("Status", user.Status),
                CreateField("Activities", activities),
                CreateField("Top Role", topRole.Mention),
                CreateField("Created At", user.CreatedAt),
                CreateField("Joined At", user.JoinedAt)
            }
        );
    }

    [SlashCommand("spotify", "Shows a user's spotify status")]
    public async Task SpotifyCommand([Summary(description: "User to show information")] SocketGuildUser? user = null)
    {
        user ??= (SocketGuildUser)User;

        if (user.Activities.FirstOrDefault(static (activity) => activity is SpotifyGame) is not SpotifyGame spotify)
        {
            await RespondEphemeralAsync("This user isn't listening Spotify now!");
            return;
        }

        await RespondWithEmbedAsync(
            title: "Listening Spotify",
            description: $"{user.Mention} is listening [{spotify.TrackTitle}]({spotify.TrackUrl}) from {spotify.AlbumTitle}\n```[ {spotify.Elapsed!.Value:mm':'ss} / {spotify.Duration!.Value:mm':'ss}]```",
            thumbnail: spotify.AlbumArtUrl
        );
    }
}