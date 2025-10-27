using Discord;

namespace Blossom.Utilities;

public static class EmbedUtility
{
    public static Embed CreateEmbed(string? title = default, string? description = default, string? thumbnail = default, Color color = default, EmbedAuthorBuilder? author = default, EmbedFooterBuilder? footer = default, EmbedFieldBuilder[]? fields = default)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithThumbnailUrl(thumbnail)
            .WithColor(color)
            .WithAuthor(author)
            .WithFooter(footer)
            .WithFields(fields ?? [])
            .Build();
    }

    public static EmbedAuthorBuilder CreateAuthor(string name, string? iconUrl = default, string? url = default)
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
            .WithValue(value ?? string.Empty)
            .WithIsInline(inline);
    }
}
