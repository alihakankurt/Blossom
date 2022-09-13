namespace Blossom.Modules;

public class InteractionModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    public static readonly Color Cherry = new(227, 30, 82);
    private readonly Random _random;

    public IServiceProvider ServiceProvider { get; }

    public Configuration Configuration { get; }

    public DiscordSocketClient Client { get; }

    public InteractionService InteractionService { get; }

    public HttpClient HttpClient { get; }

    public ISocketMessageChannel Channel => Context.Channel;

    public SocketGuild Guild => Context.Guild;

    public SocketInteraction Interaction => Context.Interaction;

    public SocketUser User => Context.User;

    public InteractionModuleBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Configuration = serviceProvider.GetService<Configuration>();
        Client = serviceProvider.GetService<DiscordSocketClient>();
        InteractionService = serviceProvider.GetService<InteractionService>();
        HttpClient = serviceProvider.GetService<HttpClient>();

        _random = serviceProvider.GetService<Random>();
    }

    public async Task RespondEphemeralAsync(string text)
    {
        await RespondAsync(text, ephemeral: true);
    }

    public async Task RespondWithEmbedAsync(params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(null, null, null, null, null, fields);
    }

    public async Task RespondWithEmbedAsync(string description, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(null, description, null, null, null, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(title, description, null, null, null, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, string thumbnailUrl, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(title, description, thumbnailUrl, null, null, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, string thumbnailUrl, Color color, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(title, description, thumbnailUrl, color, null, null, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, string thumbnailUrl, AuthorBuilder authorBuilder, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(title, description, thumbnailUrl, authorBuilder, null, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, string thumbnailUrl, FooterBuilder footerBuilder, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(title, description, thumbnailUrl, null, footerBuilder, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, string thumbnailUrl, AuthorBuilder authorBuilder, FooterBuilder footerBuilder, params FieldBuilder[] fields)
    {
        await RespondWithEmbedAsync(title, description, thumbnailUrl, Cherry, authorBuilder, footerBuilder, fields);
    }

    public async Task RespondWithEmbedAsync(string title, string description, string thumbnailUrl, Color color, AuthorBuilder authorBuilder, FooterBuilder footerBuilder, params FieldBuilder[] fields)
    {
        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithThumbnailUrl(thumbnailUrl)
            .WithColor(color)
            .WithAuthor(authorBuilder)
            .WithFooter(footerBuilder)
            .WithFields(fields)
            .Build()
        );
    }

    public string CutOff(string text, int length)
    {
        return (text.Length < length) ? text : $"{text[..(length - 3)]}...";
    }

    public int RandomNumber()
    {
        return _random.Next();
    }

    public int RandomNumber(int max)
    {
        return _random.Next(max + 1);
    }

    public int RandomNumber(int min, int max)
    {
        return _random.Next(min, max + 1);
    }

    public T Choose<T>(params T[] values)
    {
        return values[_random.Next(values.Length)];
    }

    public class AuthorBuilder : EmbedAuthorBuilder
    {
        public AuthorBuilder(string name, string iconUrl = null, string url = null)
        {
            Name = name;
            IconUrl = iconUrl;
            Url = url;
        }
    }

    public class FooterBuilder : EmbedFooterBuilder
    {
        public FooterBuilder(string text, string iconUrl)
        {
            Text = text;
            IconUrl = iconUrl;
        }
    }

    public class FieldBuilder : EmbedFieldBuilder
    {
        public FieldBuilder(string name, object value, bool inline = false)
        {
            Name = name;
            Value = value;
            IsInline = inline;
        }
    }
}
