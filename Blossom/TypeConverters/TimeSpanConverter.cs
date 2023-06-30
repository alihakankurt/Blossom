namespace Blossom.TypeConverters;

public sealed class TimeSpanConverter : TypeConverter<TimeSpan>
{
    private const string TimeSpanRegex = "^([0-9]{1,2})[:.]?([0-9]{1,2})?$";

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        Match match = Regex.Match(option.Value.ToString()!, TimeSpanRegex);
        if (!match.Success)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, reason: default));

        TimeSpan position = TimeSpan.FromSeconds(
            match.Groups[2].Success
                ? (int.Parse(match.Groups[1].Value) * 60) + int.Parse(match.Groups[2].Value)
                : int.Parse(match.Groups[1].Value)
        );
        return Task.FromResult(TypeConverterResult.FromSuccess(position));
    }

    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }
}
