namespace Blossom;

internal class Program
{
    #pragma warning disable CS8618
    private static IServiceProvider _services;
    private static DiscordSocketClient _discordClient;
    private static InteractionService _interactionService;
    #pragma warning restore CS8618

    private static async Task Main()
    {
        _services = new ServiceCollection()
            .AddSingleton<DiscordSocketConfig>(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                DefaultRetryMode = RetryMode.RetryRatelimit,
                LogGatewayIntentWarnings = false,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100,
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<InteractionServiceConfig>(new InteractionServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose,
                EnableAutocompleteHandlers = true,
            })
            .AddSingleton<InteractionService>()
            .AddSingleton<BloomConfig>(BloomConfig.Default)
            .AddSingleton<BloomNode>()
            .BuildServiceProvider();

        _discordClient = _services.GetRequiredService<DiscordSocketClient>();
        _discordClient.Log += Log;
        _discordClient.Ready += Ready;
        _discordClient.InteractionCreated += InteractionCreated;

        _interactionService = _services.GetRequiredService<InteractionService>();
        _interactionService.Log += Log;
        _interactionService.SlashCommandExecuted += SlashCommandExecuted;

        await RunAsync();
    }

    private static async Task RunAsync()
    {
        AudioService.Start(_services.GetRequiredService<BloomNode>());
        ConfigurationService.Start();

        _interactionService.AddTypeConverter<TimeSpan>(new TimeSpanConverter());
        await _interactionService.AddModuleAsync<AudioModule>(_services);
        await _interactionService.AddModuleAsync<FunModule>(_services);
        await _interactionService.AddModuleAsync<InformationModule>(_services);
        await _interactionService.AddModuleAsync<ModerationModule>(_services);

        await _discordClient.LoginAsync(TokenType.Bot, ConfigurationService.Get("Token"));
        await _discordClient.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private static Task Log(LogMessage message)
    {
        if (message.Exception is not null)
        {
            Console.WriteLine(message.Exception);
            if (message.Exception.InnerException is not null)
            {
                Console.WriteLine(message.Exception.InnerException);
            }
        }

        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({message.Source}) {message.Severity} : {((message.Exception is null) ? message.Message : message.Exception.Message)}");
        return Task.CompletedTask;
    }

    private static async Task Ready()
    {
        await _interactionService.RegisterCommandsGloballyAsync();

        BloomNode bloomNode = _services.GetRequiredService<BloomNode>();
        await bloomNode.ConnectAsync();

        await _discordClient.SetStatusAsync(ConfigurationService.Get<UserStatus>("Status"));
        await _discordClient.SetGameAsync(ConfigurationService.Get("Activity"), streamUrl: null, ConfigurationService.Get<ActivityType>("ActivityType"));
    }

    private static async Task InteractionCreated(SocketInteraction interaction)
    {
        if (interaction.Channel.GetChannelType() is ChannelType.DM or ChannelType.Group)
        {
            await interaction.RespondEphemeralAsync("I don't serve on private channels!");
            return;
        }

        SocketInteractionContext context = new(_discordClient, interaction);
        await _interactionService.ExecuteCommandAsync(context, _services);
    }

    private static async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            await context.Interaction.RespondAsync(
                result.Error switch
                {
                    InteractionCommandError.UnknownCommand => "Unknown command tried to execute!",
                    InteractionCommandError.ConvertFailed => "Passed argument failed to convert!",
                    InteractionCommandError.BadArgs => "Invalid count of arguments!",
                    InteractionCommandError.Exception => $"Command exception: `{result.ErrorReason}`",
                    InteractionCommandError.Unsuccessful => "Command execution was unsuccessful!",
                    InteractionCommandError.UnmetPrecondition => result.ErrorReason,
                    InteractionCommandError.ParseFailed => "Command failed to parse!",
                    _ => $"Error: {result.ErrorReason}",
                }
            );
        }
    }
}
