namespace Blossom.Modules;

[Name("Help Module")]
public class HelpModule : ModuleBase
{
    public HelpModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Command("help"), Summary("Sends help message.")]
    public async Task HelpCommand([Summary("Command name to get details")] string command = null)
    {
        if (command == null)
        {
            string commands = string.Join("\n\n", CommandService.Modules.Select(m => $"> {m.Name}\n{string.Join(", ", m.Commands.Select(c => $"`{c.Aliases[0]}`"))}"));
            await ReplyWithEmbedAsync($"{Context.Client.CurrentUser.Username}'s Commands",
                $"Type `{Configuration.Prefix}help <command>` to get more details for a command\nPlease do not send to me any DM messages\n\n{commands}");
            return;
        }

        if (CommandService.Commands.FirstOrDefault(c => c.Aliases.Contains(command)) is not CommandInfo commandInfo)
        {
            await ReplyAsync($"Command could not be found with `{command}`!");
            return;
        }

        string syntax = ToCodeBlock("cs", $"\n\"{Configuration.Prefix}{commandInfo.Aliases[0]} {string.Join(' ', commandInfo.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>"))}\"");
        string parameters = (commandInfo.Parameters.Count == 0) ? "No parameters required" : string.Join('\n', commandInfo.Parameters.Select(p => $"`{p.Name}`: {p.Summary}"));
        await ReplyWithEmbedAsync(
            new FieldBuilder("Syntax", syntax),
            new FieldBuilder("Summary", commandInfo.Summary),
            new FieldBuilder("Parameters", parameters),
            new FieldBuilder("Module", commandInfo.Module.Name)
        );
    }
}
