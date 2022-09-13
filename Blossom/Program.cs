ServiceProvider services = new ServiceCollection()
    .AddSingleton<DiscordSocketClient>((_) => new(new DiscordSocketConfig
    {
        AlwaysDownloadUsers = true,
        DefaultRetryMode = RetryMode.RetryRatelimit,
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations 
            | GatewayIntents.GuildWebhooks | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions 
            | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping,
        LogGatewayIntentWarnings = true,
        LogLevel = LogSeverity.Info,
        MessageCacheSize = 100,
    }))
    .AddSingleton<InteractionService>((provider) => new(provider.GetService<DiscordSocketClient>(), new InteractionServiceConfig
    {
        DefaultRunMode = RunMode.Async,
        LogLevel = LogSeverity.Info,
        EnableAutocompleteHandlers = true,
    }))
    .AddLavaNode((config) =>
    {
        config.LogSeverity = LogSeverity.Info;
        config.SelfDeaf = true;
    })
    .AddSingleton<Configuration>((_) => ConfigurationService.TryLoad())
    .AddSingleton<Random>()
    .AddSingleton<HttpClient>()
    .BuildServiceProvider();

DiscordSocketClient client = services.GetService<DiscordSocketClient>();
InteractionService interactionService = services.GetService<InteractionService>();
LavaNode lavaNode = services.GetService<LavaNode>();
Configuration configuration = services.GetService<Configuration>();

Func<LogMessage, Task> onLog = logDetails =>
{
    string message = $" [{logDetails.Severity.ToString().ToUpper()}] ({logDetails.Source}) --> {(logDetails.Exception != null ? logDetails.Exception.Message : logDetails.Message)}";

    switch (logDetails.Severity)
    {
        case LogSeverity.Critical:
            LoggerService.Critical(message);
            break;

        case LogSeverity.Error:
            LoggerService.Error(message);
            break;

        case LogSeverity.Warning:
            LoggerService.Warning(message);
            break;

        case LogSeverity.Info:
            LoggerService.Info(message);
            break;

        default:
            break;
    }

    return Task.CompletedTask;
};

client.Log += onLog;
interactionService.Log += onLog;
lavaNode.OnLog += onLog;

client.Ready += async () =>
{
    AudioService.Initialize(services);

    await interactionService.RegisterCommandsGloballyAsync();

    await client.SetStatusAsync(UserStatus.Idle);
    await client.SetGameAsync("Weywey and Yunyun on their journey.", null, ActivityType.Watching);
};

client.InteractionCreated += async (interaction) =>
{
    SocketInteractionContext context = new(client, interaction);
    await interactionService.ExecuteCommandAsync(context, services);
};

interactionService.SlashCommandExecuted += async (command, context, result) =>
{
    if (!result.IsSuccess)
    {
        await context.Interaction.RespondAsync(result.Error switch
        {
            InteractionCommandError.UnknownCommand => "Unknown command tried to execute!",
            InteractionCommandError.ConvertFailed => "Passed argument failed to convert!",
            InteractionCommandError.BadArgs => "Invalid count of arguments!",
            InteractionCommandError.Exception => $"Command exception: `{result.ErrorReason}`",
            InteractionCommandError.Unsuccessful => "Command execution was unsuccessful!",
            InteractionCommandError.UnmetPrecondition => result.ErrorReason,
            InteractionCommandError.ParseFailed => "Command failed to parse!",
            _ => $"Error: {result.ErrorReason}",
        }, ephemeral: true);
    }
};

await interactionService.AddModuleAsync<AudioModule>(services);
await interactionService.AddModuleAsync<DeveloperModule>(services);
await interactionService.AddModuleAsync<FunModule>(services);
await interactionService.AddModuleAsync<InformationModule>(services);
await interactionService.AddModuleAsync<ModerationModule>(services);

await client.LoginAsync(TokenType.Bot, configuration.Token, validateToken: true);
await client.StartAsync();
await Task.Delay(Timeout.Infinite);
