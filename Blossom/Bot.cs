using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bloom;
using Blossom.Modules;
using Blossom.Services;
using Blossom.TypeConverters;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom;

public sealed class Bot
{
    private readonly IServiceProvider _services;

    public Bot()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton(static (_) => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            DefaultRetryMode = RetryMode.RetryRatelimit,
            LogGatewayIntentWarnings = false,
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 100,
        }));

        services.AddSingleton(static (sp) => new InteractionService(sp.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose,
            EnableAutocompleteHandlers = true,
        }));

        services.AddSingleton(static (sp) => new BloomNode(sp.GetRequiredService<DiscordSocketClient>(), BloomConfiguration.Default));

        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<SomeRandomApi>();
        services.AddSingleton<AudioService>();

        services.AddSingleton<HttpClient>();

        _services = services.BuildServiceProvider();
    }

    public async Task RunAsync()
    {
        SomeRandomApi someRandomApi = _services.GetRequiredService<SomeRandomApi>();
        await someRandomApi.InitializeAsync();

        ConfigurationService configuration = _services.GetRequiredService<ConfigurationService>();
        await configuration.InitializeAsync();

        DiscordSocketClient discordClient = _services.GetRequiredService<DiscordSocketClient>();
        discordClient.Log += Log;
        discordClient.Ready += Ready;
        discordClient.InteractionCreated += InteractionCreated;

        InteractionService interactionService = _services.GetRequiredService<InteractionService>();
        interactionService.Log += Log;
        interactionService.SlashCommandExecuted += SlashCommandExecuted;

        interactionService.AddTypeConverter<TimeSpan>(new TimeSpanTypeConverter());

        await interactionService.AddModuleAsync<AudioModule>(_services);
        await interactionService.AddModuleAsync<FunModule>(_services);
        await interactionService.AddModuleAsync<InformationModule>(_services);
        await interactionService.AddModuleAsync<ModerationModule>(_services);

        await discordClient.LoginAsync(TokenType.Bot, configuration.Get("Token"));
        await discordClient.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private static Task Log(LogMessage message)
    {
        var log = new StringBuilder();
        log.Append($"[{DateTime.Now:HH:mm::ss}] ({message.Source}) {message.Severity} : {message.Message}");

        Exception? exception = message.Exception;
        while (exception is not null)
        {
            log.Append($"\n{exception.Message}\n{exception.StackTrace}");
            exception = exception.InnerException;
        }

        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private async Task Ready()
    {
        InteractionService interactionService = _services.GetRequiredService<InteractionService>();
        await interactionService.RegisterCommandsGloballyAsync();

        AudioService audioService = _services.GetRequiredService<AudioService>();
        await audioService.InitializeAsync();

        DiscordSocketClient discordClient = _services.GetRequiredService<DiscordSocketClient>();
        ConfigurationService configuration = _services.GetRequiredService<ConfigurationService>();

        UserStatus status = configuration.Get<UserStatus>("Status");
        ActivityType activityType = configuration.Get<ActivityType>("ActivityType");
        string activity = configuration.Get("Activity");

        await discordClient.SetStatusAsync(status);
        await discordClient.SetGameAsync(activity, streamUrl: null, activityType);
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        if (interaction.Channel.GetChannelType() is ChannelType.DM or ChannelType.Group)
        {
            await interaction.RespondAsync("I don't serve on private channels!", ephemeral: true);
            return;
        }

        DiscordSocketClient discordClient = _services.GetRequiredService<DiscordSocketClient>();
        InteractionService interactionService = _services.GetRequiredService<InteractionService>();

        var context = new SocketInteractionContext(discordClient, interaction);
        await interactionService.ExecuteCommandAsync(context, _services);
    }

    private static async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
            return;

        await context.Interaction.RespondAsync($"{result.Error!.Value}: {result.ErrorReason}");
    }
}
