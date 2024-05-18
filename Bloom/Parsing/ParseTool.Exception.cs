using System;
using System.Text.Json.Nodes;

namespace Bloom.Parsing;

internal static partial class ParseTool
{
    internal static BloomException ParseException(JsonNode node)
    {
        string? message = node["message"]?.GetValue<string>();
        BloomExceptionSeverity severity = Enum.Parse<BloomExceptionSeverity>(node["severity"]!.GetValue<string>(), ignoreCase: true);
        string cause = node["cause"]!.GetValue<string>();

        return new BloomException(message, severity, cause);
    }
}
