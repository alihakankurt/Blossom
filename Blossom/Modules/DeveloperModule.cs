namespace Blossom.Modules;

public sealed class DeveloperModule : BaseInteractionModule
{
    public DeveloperModule(IServiceProvider services) : base(services)
    {
    }

    [SlashCommand("ascii", "Returns the ASCII value of a character")]
    public async Task AsciiCommand([Summary(description: "The value to convert")] char character)
    {
        await RespondAsync($"`{character}`: {(int)character}");
    }

    [SlashCommand("binary", "Returns the binary value of a decimal number")]
    public async Task BinaryCommand([Summary(description: "The value to convert")] int value)
    {
        await RespondAsync($"`{value}`: {Convert.ToString(value, 2)}");
    }

    [SlashCommand("octal", "Returns the octal value of a decimal number")]
    public async Task OctalCommand([Summary(description: "The value to convert")] int value)
    {
        await RespondAsync($"`{value}`: {Convert.ToString(value, 8)}");
    }

    [SlashCommand("hexal", "Returns the hexal value of a decimal number")]
    public async Task HexalCommand([Summary(description: "The value to convert")] int value)
    {
        await RespondAsync($"`{value}`: {Convert.ToString(value, 16)}");
    }
}
