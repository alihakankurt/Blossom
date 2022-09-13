namespace Blossom.Services;

public static class LoggerService
{
    public static void Critical(string content)
    {
        Log(content, ConsoleColor.Magenta);
    }

    public static void Error(string content)
    {
        Log(content, ConsoleColor.Red);
    }

    public static void Warning(string content)
    {
        Log(content, ConsoleColor.Yellow);
    }

    public static void Info(string content)
    {
        Log(content, ConsoleColor.Cyan);
    }

    public static void Log(string content)
    {
        Log(content, ConsoleColor.White);
    }

    private static void Log(string content, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {content}");
        Console.ResetColor();
    }
}
