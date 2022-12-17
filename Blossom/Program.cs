namespace Blossom;

public static class Program
{
    #nullable disable

    private static ServiceProvider _services;
    private static DiscordSocketClient _discordClient;
    private static InteractionService _interactionService;
    private static AudioService _audioService;
    private static ConfigurationService _configurationService;
    private static IMessageChannel _modmailChannel;
    
    #nullable enable

    private static async Task Main(string[] args)
    {
        SetupServices();
        await Run();
    }

    private static void SetupServices()
    {        
        _services = new ServiceCollection()
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
                })
            )
            .AddSingleton<InteractionService>(static (services) => new InteractionService(services.GetRequiredService<DiscordSocketClient>(),
                new InteractionServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose,
                    EnableAutocompleteHandlers = true,
                })
            )
            .AddLavaNode<LavaPlayer, LavaTrack>(static (config) =>
            {
                config.SelfDeaf = true;
            })
            .AddSingleton<ILogger<LavaNode<LavaPlayer, LavaTrack>>>(static (_) => NullLogger<LavaNode<LavaPlayer, LavaTrack>>.Instance)
            .AddSingleton<AudioService>()
            .AddSingleton<ConfigurationService>()
            .BuildServiceProvider();
        

        _discordClient = _services.GetRequiredService<DiscordSocketClient>();
        _discordClient.Log += Log;
        _discordClient.Ready += Ready;
        _discordClient.InteractionCreated += InteractionCreated;
        _discordClient.ModalSubmitted += ModalSubmitted;

        _interactionService = _services.GetRequiredService<InteractionService>();
        _interactionService.Log += Log;
        _interactionService.SlashCommandExecuted += SlashCommandExecuted;
    }

    private static Task Log(LogMessage message)
    {
        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] ({message.Source}) {message.Severity} : {((message.Exception is null) ? message.Message : message.Exception.Message)}");
        return Task.CompletedTask;
    }

    private static async Task Ready()
    {
        _modmailChannel = (IMessageChannel)await _discordClient.GetChannelAsync(_configurationService.Get<ulong>("ModmailChannel"));

        _audioService = _services.GetRequiredService<AudioService>();
        await _audioService.ConnectAsync();

        await _interactionService.RegisterCommandsGloballyAsync();

        await _discordClient.SetStatusAsync(_configurationService.Get<UserStatus>("Status"));
        await _discordClient.SetGameAsync(_configurationService.Get("Activity"), streamUrl: null, _configurationService.Get<ActivityType>("ActivityType"));
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

    private static async Task ModalSubmitted(SocketModal modal)
    {
        if (modal.Data.CustomId is "modmailModal" && _modmailChannel is not null)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            await _modmailChannel.SendEmbedAsync(
                description: components.First(static (component) => component.CustomId is "modmailContent").Value,
                color: BaseInteractionModule.Cherry,
                author: Extensions.CreateAuthor(modal.User.ToString(), modal.User.GetAvatarUrl()),
                footer: Extensions.CreateFooter($"Sent at {TimeOnly.FromDateTime(DateTime.Now)} from {_discordClient.GetGuild(modal.GuildId!.Value)}")
            );

            await modal.RespondAsync("Modmail submitted. Thanks for your feedback.");
        }
    }

    private static async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            await context.Interaction.RespondEphemeralAsync(
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

    private static async Task Run()
    {
        _configurationService = _services.GetRequiredService<ConfigurationService>();
        _configurationService.LoadFromFile();

        _interactionService.AddTypeConverter<TimeSpan>(new TimeSpanConverter());

        await _interactionService.AddModuleAsync<AudioModule>(_services);
        await _interactionService.AddModuleAsync<DeveloperModule>(_services);
        await _interactionService.AddModuleAsync<FunModule>(_services);
        await _interactionService.AddModuleAsync<HelpModule>(_services);
        await _interactionService.AddModuleAsync<InformationModule>(_services);
        await _interactionService.AddModuleAsync<ModerationModule>(_services);

        await _discordClient.LoginAsync(TokenType.Bot, _configurationService.Get("Token"), validateToken: true);
        await _discordClient.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
}
