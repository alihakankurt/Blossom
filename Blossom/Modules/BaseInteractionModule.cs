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

    public Task RespondEphemeralAsync(string text)
    {
        return Interaction.RespondEphemeralAsync(text);
    }

    public Task RespondWithEmbedAsync(string? title = default, string? description = default, string? thumbnail = default, EmbedAuthorBuilder? author = default, EmbedFooterBuilder? footer = default, EmbedFieldBuilder[]? fields = default)
    {
        return Interaction.RespondWithEmbedAsync(title, description, thumbnail, Cherry, author, footer, fields);
    }

    public static EmbedAuthorBuilder? CreateAuthor(string name, string? iconUrl = default, string? url = default)
    {
        return Extensions.CreateAuthor(name, iconUrl, url);
    }

    public static EmbedFooterBuilder CreateFooter(string text, string? iconUrl = default)
    {
        return Extensions.CreateFooter(text, iconUrl);
    }

    public static EmbedFieldBuilder CreateField(string name, object? value, bool inline = false)
    {
        return Extensions.CreateField(name, value, inline);
    }
}
