using Discord;

namespace Blossom.Services;

public sealed class BlossomConfig
{
    public string Token { get; set; } = string.Empty;
    public UserStatus UserStatus { get; set; } = UserStatus.Online;
    public ActivityType ActivityType { get; set; } = ActivityType.CustomStatus;
    public string Activity { get; set; } = string.Empty;
    public string? StreamUrl { get; set; } = null;
}

public static class ConfigurationService
{
    public const char Seperator = ':';
    public const string FileName = "blossom.cfg";

    public static BlossomConfig Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Configuration could not be found! Make sure you have one at: {path}");
        }

        var config = new BlossomConfig();

        using var reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            ReadOnlySpan<char> chars = line;

            int index = chars.IndexOf(Seperator);
            if (index == -1)
            {
                throw new InvalidOperationException($"Invalid setting found in configuration file! Make sure you seperated the key and value with {Seperator}");
            }

            ReadOnlySpan<char> key = chars[..index].Trim();
            ReadOnlySpan<char> value = chars[(index + 1)..].Trim();

            switch (key)
            {
                case nameof(BlossomConfig.Token):
                    config.Token = new string(value);
                    break;
                case nameof(BlossomConfig.UserStatus):
                    config.UserStatus = Enum.Parse<UserStatus>(value);
                    break;
                case nameof(BlossomConfig.ActivityType):
                    config.ActivityType = Enum.Parse<ActivityType>(value);
                    break;
                case nameof(BlossomConfig.Activity):
                    config.Activity = new string(value);
                    break;
                case nameof(BlossomConfig.StreamUrl):
                    config.StreamUrl = new string(value);
                    break;
            }
        }

        return config;
    }
}
