ServiceProvider services = new ServiceCollection()
    .AddSingleton<DiscordSocketClient>((_) => new DiscordSocketClient(new DiscordSocketConfig
    {
        AlwaysDownloadUsers = true,
        DefaultRetryMode = RetryMode.RetryRatelimit,
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations
            | GatewayIntents.GuildWebhooks | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions
            | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessageTyping,
        LogGatewayIntentWarnings = false,
        LogLevel = LogSeverity.Verbose,
        MessageCacheSize = 100,
    }))
    .AddSingleton<InteractionService>(static (services) => new InteractionService(services.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig
    {
        DefaultRunMode = RunMode.Async,
        LogLevel = LogSeverity.Verbose,
        EnableAutocompleteHandlers = true,
    }))
    .AddLavaNode<LavaPlayer, LavaTrack>(static (config) =>
    {
        config.SelfDeaf = true;
    })
    .AddSingleton<ILogger<LavaNode<LavaPlayer, LavaTrack>>>(static (_) => NullLogger<LavaNode<LavaPlayer, LavaTrack>>.Instance)
    .AddSingleton<AudioService>()
    .AddSingleton<ConfigurationService>()
    .BuildServiceProvider();

DiscordSocketClient discordClient = services.GetRequiredService<DiscordSocketClient>();
InteractionService interactionService = services.GetRequiredService<InteractionService>();
AudioService audioService = services.GetRequiredService<AudioService>();
ConfigurationService configurationService = services.GetRequiredService<ConfigurationService>();

Func<LogMessage, Task> logged = static (e) =>
{
    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({e.Source}) {e.Severity} : {((e.Exception is null) ? e.Message : e.Exception.Message)}");
    return Task.CompletedTask;
};

discordClient.Log += logged;
interactionService.Log += logged;

discordClient.Ready += async () =>
{
    await audioService.ConnectAsync();

    await interactionService.RegisterCommandsGloballyAsync();

    await discordClient.SetStatusAsync(configurationService.Get<UserStatus>("Status"));
    await discordClient.SetGameAsync(configurationService.Get("Activity"), streamUrl: null, configurationService.Get<ActivityType>("ActivityType"));
};

discordClient.InteractionCreated += async (interaction) =>
{
    SocketInteractionContext context = new(discordClient, interaction);
    await interactionService.ExecuteCommandAsync(context, services);
};

interactionService.SlashCommandExecuted += static async (command, context, result) =>
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
            },
            ephemeral: true
        );
    }
};

configurationService.LoadFromFile();

interactionService.AddTypeConverter<TimeSpan>(new TimeSpanConverter());

await interactionService.AddModuleAsync<AudioModule>(services);
await interactionService.AddModuleAsync<DeveloperModule>(services);
await interactionService.AddModuleAsync<FunModule>(services);
await interactionService.AddModuleAsync<InformationModule>(services);
await interactionService.AddModuleAsync<ModerationModule>(services);

await discordClient.LoginAsync(TokenType.Bot, configurationService.Get("Token"), validateToken: true);
await discordClient.StartAsync();
await Task.Delay(Timeout.Infinite);
