namespace Blossom.Services;

public static class ConfigurationService
{
    public const string FileName = "configuration.cfg";

    public static Configuration TryLoad()
    {
        try
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(FileName)
                .Build();

            return new(configurationRoot["Token"], configurationRoot["Version"]);
        }
        catch
        {
            Environment.Exit(1);
            throw new("Configuration couldn't be loaded...");
        }
    }
}
