namespace Blossom;

public class Configuration
{
    public string Token { get; }
    public string Prefix { get; }
    public string Version { get; }
    public string JsonFile { get; }
    public ulong OwnerId { get; }

    public Configuration()
    {
        try
        {
            JsonFile = "configuration.cfg";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(JsonFile)
                .Build();

            Token = configuration["Token"];
            Prefix = configuration["Prefix"];
            Version = configuration["Version"];
            OwnerId = ulong.Parse(configuration["OwnerId"]);
        }
        catch
        {
            Logger.Error("Configuration could not be loaded...");
            Environment.Exit(1);
        }
    }
}
