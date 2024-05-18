using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blossom.Services;
using Blossom.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

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
        Embed embed = EmbedUtility.CreateEmbed(
            description: $"{Client.CurrentUser.Mention}'s Commands",
            color: Cherry,
            fields: [..InteractionService.Modules
                .Select(static (module) => EmbedUtility.CreateField(
                    $"> {module.Name}",
                    string.Join('\n', module.SlashCommands.Select(static (command) => $"`{command.Name}`: {command.Description}"))
                )
            )]
        );

        await RespondAsync(embed: embed);
    }

    [SlashCommand("status", "Shows my current status")]
    public async Task StatusCommand()
    {
        string dotNetVersion = Environment.Version.ToString(3);
        string discordNetVersion = typeof(DiscordSocketClient).Assembly.GetName().Version!.ToString(3);

        ConfigurationService configuration = Services.GetRequiredService<ConfigurationService>();
        string botVersion = configuration.Get("Version");

        string latency = $"{((Client.Latency < 100) ? GreenCircle : (Client.Latency < 250) ? YelloCircle : RedCircle)} {Client.Latency} MS";
        Process currentProcess = Process.GetCurrentProcess();
        string ramUsage = $"{currentProcess.PrivateMemorySize64 / 1048576} MB";
        string cpuTime = $"{currentProcess.TotalProcessorTime.TotalMilliseconds} MS";

        Embed embed = EmbedUtility.CreateEmbed(
            description: "Current Status",
            color: Cherry,
            fields:
            [
                EmbedUtility.CreateField(".NET Version", dotNetVersion),
                EmbedUtility.CreateField("Discord.NET Version", discordNetVersion),
                EmbedUtility.CreateField("Bot Version", botVersion),
                EmbedUtility.CreateField("Latency", latency),
                EmbedUtility.CreateField("RAM Usage", ramUsage),
                EmbedUtility.CreateField("CPU Time", cpuTime),
            ]
        );

        await RespondAsync(embed: embed);
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

        Embed embed = EmbedUtility.CreateEmbed(
            title: "Guild Information",
            description: Guild.Description,
            thumbnail: Guild.BannerUrl,
            color: Cherry,
            author: EmbedUtility.CreateAuthor(Guild.Name, Guild.IconUrl),
            fields:
            [
                EmbedUtility.CreateField("Id", Guild.Id),
                EmbedUtility.CreateField("Owner", Guild.Owner.Mention),
                EmbedUtility.CreateField("Created At", Guild.CreatedAt),
                EmbedUtility.CreateField("Channels", $"📁 {categoryChannels}\n💬 {textChannels}\n🔊 {voiceChannels}\n🎙️ {stageChannels}\n🧵 {threadChannels}"),
                EmbedUtility.CreateField("Members", $"{GreenCircle} {onlineMembers}\n{YelloCircle} {idleMembers}\n{RedCircle} {dndMembers}\n{BlackCircle} {offlineMembers}"),
                EmbedUtility.CreateField("Emotes", Guild.Emotes.Count),
                EmbedUtility.CreateField("System Channel", Guild.SystemChannel?.Mention ?? NoSystemChannel),
                EmbedUtility.CreateField("Rules Channel", Guild.RulesChannel?.Mention ?? NoRulesChannel),
                EmbedUtility.CreateField("AFK Channel", Guild.AFKChannel?.Mention ?? NoAFKChannel),
                EmbedUtility.CreateField("AFK Timeout", Guild.AFKTimeout),
                EmbedUtility.CreateField("Premium Tier", Guild.PremiumTier),
            ]
        );

        await RespondAsync(embed: embed);
    }

    [SlashCommand("user", "Shows a user's information")]
    public async Task UserCommand([Summary(description: "User to show information")] SocketGuildUser? user = null)
    {
        user ??= (SocketGuildUser)User;

        string activities = NoActivity;
        if (user.Activities.Count > 0)
            activities = string.Join('\n', user.Activities.Select(static (activity) => $"`{activity.Type} {activity.Name}` {activity.Details}"));

        SocketRole topRole = user.Roles.MaxBy(static (role) => role.Position)!;

        Embed embed = EmbedUtility.CreateEmbed(
            title: "User Information",
            author: EmbedUtility.CreateAuthor(user.DisplayName, user.GetAvatarUrl()),
            color: Cherry,
            fields:
            [
                EmbedUtility.CreateField("Id", user.Id),
                EmbedUtility.CreateField("Username", user.Username),
                EmbedUtility.CreateField("Display Name", user.GlobalName),
                EmbedUtility.CreateField("Is Bot?", user.IsBot),
                EmbedUtility.CreateField("Status", user.Status),
                EmbedUtility.CreateField("Activities", activities),
                EmbedUtility.CreateField("Top Role", topRole.Mention),
                EmbedUtility.CreateField("Created At", user.CreatedAt),
                EmbedUtility.CreateField("Joined At", user.JoinedAt)
            ]
        );

        await RespondAsync(embed: embed);
    }

    [SlashCommand("spotify", "Shows a user's spotify status")]
    public async Task SpotifyCommand([Summary(description: "User to show information")] SocketGuildUser? user = null)
    {
        user ??= (SocketGuildUser)User;

        if (user.Activities.FirstOrDefault(static (activity) => activity is SpotifyGame) is not SpotifyGame spotify)
        {
            await RespondAsync("This user isn't listening Spotify now!", ephemeral: true);
            return;
        }

        Embed embed = EmbedUtility.CreateEmbed(
            title: "Listening Spotify",
            description: $"{user.Mention} is listening [{spotify.TrackTitle}]({spotify.TrackUrl}) from {spotify.AlbumTitle}\n```[ {spotify.Elapsed!.Value:mm':'ss} / {spotify.Duration!.Value:mm':'ss}]```",
            thumbnail: spotify.AlbumArtUrl,
            color: Cherry
        );

        await RespondAsync(embed: embed);
    }
}
