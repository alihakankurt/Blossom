namespace Blossom;

public class ModuleBase : ModuleBase<SocketCommandContext>
{
    public static readonly Color Cherry = new(227, 30, 82);

    private readonly Random random;

    public IServiceProvider ServiceProvider { get; }
    public Configuration Configuration { get; }
    public DiscordSocketClient Client { get; }
    public CommandService CommandService { get; }
    public HttpClient HttpClient { get; }
    public ISocketMessageChannel Channel => Context.Channel;
    public SocketGuild Guild => Context.Guild;
    public SocketUserMessage Message => Context.Message;
    public SocketUser User => Context.User;

    public ModuleBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Configuration = serviceProvider.GetService<Configuration>();
        Client = serviceProvider.GetService<DiscordSocketClient>();
        CommandService = serviceProvider.GetService<CommandService>();
        HttpClient = serviceProvider.GetService<HttpClient>();

        random = serviceProvider.GetService<Random>();
    }

    public async Task ReplyWithEmbedAsync(params FieldBuilder[] fields)
    {
        await ReplyWithEmbedAsync(null, null, null, null, null, fields);
    }

    public async Task ReplyWithEmbedAsync(string description, params FieldBuilder[] fields)
    {
        await ReplyWithEmbedAsync(null, description, null, null, null, fields);
    }

    public async Task ReplyWithEmbedAsync(string title, string description, params FieldBuilder[] fields)
    {
        await ReplyWithEmbedAsync(title, description, null, null, null, fields);
    }

    public async Task ReplyWithEmbedAsync(string title, string description, string thumbnailUrl, params FieldBuilder[] fields)
    {
        await ReplyWithEmbedAsync(title, description, thumbnailUrl, null, null, fields);
    }

    public async Task ReplyWithEmbedAsync(string title, string description, string thumbnailUrl, AuthorBuilder authorBuilder, params FieldBuilder[] fields)
    {
        await ReplyWithEmbedAsync(title, description, thumbnailUrl, authorBuilder, null, fields);
    }

    public async Task ReplyWithEmbedAsync(string title, string description, string thumbnailUrl, FooterBuilder footerBuilder, params FieldBuilder[] fields)
    {
        await ReplyWithEmbedAsync(title, description, thumbnailUrl, null, footerBuilder, fields);
    }

    public async Task ReplyWithEmbedAsync(string title, string description, string thumbnailUrl, AuthorBuilder authorBuilder, FooterBuilder footerBuilder, params FieldBuilder[] fields)
    {
        await ReplyAsync(embed: new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithThumbnailUrl(thumbnailUrl)
            .WithColor(Cherry)
            .WithAuthor(authorBuilder)
            .WithFooter(footerBuilder)
            .WithFields(fields)
            .Build()
        );
    }

    public int RandomNumber()
    {
        return random.Next();
    }

    public int RandomNumber(int max)
    {
        return random.Next(max + 1);
    }

    public int RandomNumber(int min, int max)
    {
        return random.Next(min, max + 1);
    }

    public T Choose<T>(params T[] values)
    {
        return values[random.Next(values.Length)];
    }

    public string ToCodeBlock(string language, string code)
    {
        return $"```{language}\n{code}```";
    }

    public string RemoveCodeBlock(string codeblock)
    {
        return string.Join('\n', codeblock.Split('\n')[1..]).TrimEnd('`');
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
