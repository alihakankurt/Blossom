DiscordSocketClient client = new(new DiscordSocketConfig
{
    AlwaysDownloadUsers = true,
    DefaultRetryMode = RetryMode.RetryRatelimit,
    GatewayIntents = GatewayIntents.All,
    LogGatewayIntentWarnings = true,
    LogLevel = LogSeverity.Info,
    MessageCacheSize = 100,
});

CommandService commandService = new(new CommandServiceConfig
{
    CaseSensitiveCommands = true,
    DefaultRunMode = RunMode.Async,
    IgnoreExtraArgs = true,
    LogLevel = LogSeverity.Info
});

LavaNode lavaNode = new(client, new LavaConfig
{
    LogSeverity = LogSeverity.Info,
    SelfDeaf = true,
});

Configuration configuration = new();

ServiceProvider provider = new ServiceCollection()
    .AddSingleton(client)
    .AddSingleton(commandService)
    .AddSingleton(lavaNode)
    .AddSingleton(configuration)
    .AddSingleton<Random>()
    .AddSingleton<HttpClient>()
    .BuildServiceProvider();

Func<LogMessage, Task> log = m =>
{
    Logger.Log($" [{m.Severity.ToString().ToUpper()}] ({m.Source}) --> {(m.Exception != null ? m.Exception : m.Message)}",
        m.Severity switch
        {
            LogSeverity.Critical => ConsoleColor.Magenta,
            LogSeverity.Error => ConsoleColor.Magenta,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Info => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        });

    return Task.CompletedTask;
};

client.Log += log;
commandService.Log += log;
lavaNode.OnLog += log;

client.Ready += async () =>
{
    try
    {
        await lavaNode.ConnectAsync();
    }
    catch
    {
    }

    await client.SetStatusAsync(UserStatus.Idle);
    await client.SetGameAsync("Spotify", null, ActivityType.Listening);
};

client.MessageReceived += async message =>
{
    if (message.Author.IsBot || message.Channel is IDMChannel)
    {
        return;
    }

    int argPos = 0;
    SocketUserMessage socketUserMessage = message as SocketUserMessage;

    if (!socketUserMessage.HasStringPrefix(configuration.Prefix, ref argPos))
    {
        return;
    }

    SocketCommandContext context = new(client, socketUserMessage);

    IResult result = await commandService.ExecuteAsync(context, argPos, provider);

    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
    {
        await context.Channel.SendMessageAsync(result.Error switch
        {
            CommandError.ParseFailed => "Invalid parameters passed to execute!",
            CommandError.BadArgCount => "Invalid count of parameters!",
            CommandError.ObjectNotFound => "Could not find any Discord object!",
            CommandError.MultipleMatches => "Multiple commands were found. Please be more specific!",
            CommandError.UnmetPrecondition => "Not enough authority to execute this command!",
            CommandError.Exception => $"Command exception: `{result.ErrorReason}`",
            CommandError.Unsuccessful => "Command could not be executed!",
            _ => $"Error: {result.ErrorReason}"
        });
    }
};

await commandService.AddModuleAsync<AudioModule>(provider);
await commandService.AddModuleAsync<DeveloperModule>(provider);
await commandService.AddModuleAsync<FunModule>(provider);
await commandService.AddModuleAsync<HelpModule>(provider);
await commandService.AddModuleAsync<InformationModule>(provider);
await commandService.AddModuleAsync<ModerationModule>(provider);
await commandService.AddModuleAsync<OwnerModule>(provider);

await client.LoginAsync(TokenType.Bot, configuration.Token, true);
await client.StartAsync();
await Task.Delay(Timeout.Infinite);
