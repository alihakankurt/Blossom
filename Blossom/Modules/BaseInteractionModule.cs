using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Blossom.Modules;

public abstract class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    public static readonly Color Cherry;

    protected IServiceProvider Services { get; }
    protected DiscordSocketClient Client { get; }
    protected InteractionService InteractionService { get; }
    protected ISocketMessageChannel Channel => Context.Channel;
    protected SocketGuild Guild => Context.Guild;
    protected SocketInteraction Interaction => Context.Interaction;
    protected SocketUser User => Context.User;
    protected IVoiceState? VoiceState => User as IVoiceState;

    static BaseInteractionModule()
    {
        Cherry = new Color(227, 30, 82);
    }

    public BaseInteractionModule(IServiceProvider services)
    {
        Services = services;
        Client = services.GetRequiredService<DiscordSocketClient>();
        InteractionService = services.GetRequiredService<InteractionService>();
    }
}
