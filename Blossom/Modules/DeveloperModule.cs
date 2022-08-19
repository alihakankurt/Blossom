namespace Blossom.Modules;

[Name("Developer Module")]
public class DeveloperModule : ModuleBase
{
    public DeveloperModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Command("ascii"), Summary("Returns the ASCII value of a character")]
    public async Task AsciiCommand([Summary("The value to convert")] char character)
    {
        await ReplyAsync($"`{character}`: {(int)character}");
    }

    [Command("binary"), Summary("Returns the binary value of a decimal number")]
    public async Task BinaryCommand([Summary("The value to convert")] int value)
    {
        await ReplyAsync($"`{value}`: {Convert.ToString(value, 2)}");
    }

    [Command("octal"), Summary("Returns the octal value of a decimal number")]
    public async Task OctalCommand([Summary("The value to convert")] int value)
    {
        await ReplyAsync($"`{value}`: {Convert.ToString(value, 8)}");
    }

    [Command("hexal"), Summary("Returns the hexal value of a decimal number")]
    public async Task HexalCommand([Summary("The value to convert")] int value)
    {
        await ReplyAsync($"`{value}`: {Convert.ToString(value, 16)}");
    }
}
