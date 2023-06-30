namespace Blossom.Services;

public static class ConfigurationService
{
    private const string FileName = "config.cfg";
    private const string Seperator = "=";

    private static readonly Dictionary<string, string> _keyValuePairs = new();

    public static void Start()
    {
        string path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Configuration file not found! Make sure you have a file named {FileName}");

        Array.ForEach(File.ReadAllLines(path), (line) =>
        {
            string[] lines = line.Split(Seperator).ToArray();
            if (lines.Length is not 2)
                throw new InvalidOperationException($"Invalid matching on {line} in configuration file! Make sure you seperated the key and value with {Seperator}");

            _keyValuePairs.Add(lines[0], lines[1]);
        });
    }

    public static string Get(string key)
    {
        return _keyValuePairs.TryGetValue(key, out string? value)
            ? value
            : throw new InvalidOperationException($"Couldn't find {key} in configuration");
    }

    public static T Get<T>(string key)
    {
        string value = Get(key);

        System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
        return converter.CanConvertFrom(typeof(string))
            ? (T)converter.ConvertFrom(value)!
            : throw new InvalidCastException($"Couldn't convert {value} to {typeof(T)}");
    }
}
