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
    private readonly DiscordSocketClient _discordClient;
    private readonly InteractionService _interactionService;

    private BlossomConfig _config = null!;

    public Bot()
    {
        _discordClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            DefaultRetryMode = RetryMode.RetryRatelimit,
            LogGatewayIntentWarnings = false,
            LogLevel = LogSeverity.Verbose,
            MessageCacheSize = 100,
        });

        _discordClient.Log += Log;
        _discordClient.Ready += Ready;
        _discordClient.InteractionCreated += InteractionCreated;

        _interactionService = new InteractionService(_discordClient, new InteractionServiceConfig
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose,
            EnableAutocompleteHandlers = true,
        });

        _interactionService.Log += Log;
        _interactionService.SlashCommandExecuted += SlashCommandExecuted;
        _interactionService.AddTypeConverter<TimeSpan>(new TimeSpanTypeConverter());

        _services = new ServiceCollection()
            .AddSingleton<DiscordSocketClient>(_discordClient)
            .AddSingleton<InteractionService>(_interactionService)
            .AddSingleton<BloomConfig>()
            .AddSingleton<BloomNode>()
            .AddSingleton<SomeRandomApi>()
            .AddSingleton<AudioService>()
            .AddSingleton<HttpClient>()
            .BuildServiceProvider();
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _config = ConfigurationService.Load();

        await _interactionService.AddModuleAsync<AudioModule>(_services);
        await _interactionService.AddModuleAsync<FunModule>(_services);
        await _interactionService.AddModuleAsync<InformationModule>(_services);
        await _interactionService.AddModuleAsync<ModerationModule>(_services);

        await _discordClient.LoginAsync(TokenType.Bot, _config.Token);
        await _discordClient.StartAsync();

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await _discordClient.StopAsync();
            await _discordClient.LogoutAsync();
        }
    }

    private async Task Ready()
    {
        await _interactionService.RegisterCommandsGloballyAsync();

        var audioService = _services.GetRequiredService<AudioService>();
        await audioService.InitializeAsync();

        await _discordClient.SetStatusAsync(_config.UserStatus);
        await _discordClient.SetGameAsync(_config.Activity, _config.StreamUrl, _config.ActivityType);
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        if (interaction.Channel.GetChannelType() is ChannelType.DM or ChannelType.Group)
        {
            await interaction.RespondAsync("I don't serve on private channels!", ephemeral: true);
            return;
        }

        var context = new SocketInteractionContext(_discordClient, interaction);
        await _interactionService.ExecuteCommandAsync(context, _services);
    }

    private static Task Log(LogMessage message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }

    private static async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
            return;

        await context.Interaction.RespondAsync($"{result.Error!.Value}: {result.ErrorReason}");
    }
}
