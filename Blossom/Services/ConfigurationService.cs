using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Blossom.Services;

public sealed class ConfigurationService : IService
{
    public const char Seperator = '=';
    public const string FileName = "config.cfg";

    private readonly Dictionary<string, string> _settings;

    public ConfigurationService()
    {
        _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        string path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Configuration file not found! Make sure you have a file named {FileName}");

        string[] lines = await File.ReadAllLinesAsync(path, cancellationToken);
        foreach (string line in lines)
        {
            int index = line.AsSpan().IndexOf(Seperator);
            if (index == -1)
            {
                throw new InvalidOperationException($"Invalid matching on {line} in configuration file! Make sure you seperated the key and value with {Seperator}");
            }

            string key = line[..index];
            string value = line[(index + 1)..];
            _settings.Add(key, value);
        }
    }

    public string Get(string key)
    {
        if (!_settings.TryGetValue(key, out string? value))
            throw new InvalidOperationException($"Couldn't find {key} in configuration");

        return value;
    }

    public TValue Get<TValue>(string key) where TValue : notnull
    {
        string value = Get(key);
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));
        if (converter.CanConvertFrom(typeof(string)))
            return (TValue)converter.ConvertFromString(value)!;

        throw new InvalidOperationException($"Couldn't convert {value} to {typeof(TValue).Name}");
    }
}
