namespace Blossom;

public static class Extensions
{
    private static readonly Random Randomizer;

    static Extensions()
    {
        Randomizer = new Random();
    }

    public static string CutOff(this string self, int maxLength)
    {
        return (self.Length < maxLength) ? self : $"{self[..(maxLength - 3)]}...";
    }

    public static int Random(int start, int end)
    {
        return Randomizer.Next(start, end);
    }

    public static T Choose<T>(this ICollection<T> self)
    {
        int index = Randomizer.Next(self.Count);
        return self.ElementAt(index);
    }

    public static SocketRole GetTopRole(this SocketGuildUser self)
    {
        return self.Roles.OrderByDescending((role) => role).First();
    }

    public static Task RespondEphemeralAsync(this IDiscordInteraction self, string text)
    {
        return self.RespondAsync(text, ephemeral: true);
    }

    public static Task RespondWithEmbedAsync(this IDiscordInteraction self, string? title = default, string? description = default, string? thumbnail = default, Color color = default, EmbedAuthorBuilder? author = default, EmbedFooterBuilder? footer = default, EmbedFieldBuilder[]? fields = default)
    {
        Embed embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithThumbnailUrl(thumbnail)
            .WithColor(color)
            .WithAuthor(author)
            .WithFooter(footer)
            .WithFields(fields ?? Array.Empty<EmbedFieldBuilder>())
            .Build();

        return self.RespondAsync(embed: embed);
    }

    public static Task SendEmbedAsync(this IMessageChannel self, string? title = default, string? description = default, string? thumbnail = default, Color color = default, EmbedAuthorBuilder? author = default, EmbedFooterBuilder? footer = default, EmbedFieldBuilder[]? fields = default)
    {
        Embed embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithThumbnailUrl(thumbnail)
            .WithColor(color)
            .WithAuthor(author)
            .WithFooter(footer)
            .WithFields(fields ?? Array.Empty<EmbedFieldBuilder>())
            .Build();

        return self.SendMessageAsync(embed: embed);
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
            .WithValue(value ?? "null")
            .WithIsInline(inline);
    }
}
