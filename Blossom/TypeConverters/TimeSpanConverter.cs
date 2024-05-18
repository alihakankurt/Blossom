using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Blossom.TypeConverters;

public sealed partial class TimeSpanTypeConverter : TypeConverter<TimeSpan>
{
    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        Match match = GetTimeSpanRegex().Match(option.Value.ToString()!);
        if (!match.Success)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Invalid time span format."));

        int minutes = 0;
        int seconds = int.Parse(match.Groups[1].Value);

        if (match.Groups[2].Success)
        {
            minutes = seconds;
            seconds = int.Parse(match.Groups[2].Value);
        }

        if (seconds > 59)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Seconds must be less than 60."));

        if (minutes > 59)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Minutes must be less than 60."));

        TimeSpan value = TimeSpan.FromSeconds(seconds) + TimeSpan.FromMinutes(minutes);
        return Task.FromResult(TypeConverterResult.FromSuccess(value));
    }

    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }

    [GeneratedRegex("^([0-9]{1,2})[:.]?([0-9]{1,2})?$")]
    private static partial Regex GetTimeSpanRegex();
}
