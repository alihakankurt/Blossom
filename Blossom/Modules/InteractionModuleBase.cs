namespace Blossom.Modules;

public abstract class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string FieldValueNull = "null";

    protected static readonly Color Cherry;

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
        return RespondAsync(text, ephemeral: true);
    }

    public Task RespondWithEmbedAsync(string? title = default, string? description = default, string? thumbnail = default, EmbedAuthorBuilder? author = default, EmbedFooterBuilder? footer = default, EmbedFieldBuilder[]? fields = default)
    {
        Embed embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithThumbnailUrl(thumbnail)
            .WithColor(Cherry)
            .WithAuthor(author)
            .WithFooter(footer)
            .WithFields(fields ?? Array.Empty<EmbedFieldBuilder>())
            .Build();

        return RespondAsync(embed: embed);
    }

    public static EmbedAuthorBuilder? CreateAuthor(string name, string? iconUrl = default, string? url = default)
    {
        return new EmbedAuthorBuilder()
            .WithName(name)
            .WithIconUrl(iconUrl)
            .WithUrl(url);
    }

    public static EmbedFooterBuilder CreateFooter(string text, string? iconUrl = default)
    {
        return new EmbedFooterBuilder()
            .WithText(text)
            .WithIconUrl(iconUrl);
    }

    public static EmbedFieldBuilder CreateField(string name, object? value, bool inline = false)
    {
        return new EmbedFieldBuilder()
            .WithName(name)
            .WithValue(value ?? FieldValueNull)
            .WithIsInline(inline);
    }
}
