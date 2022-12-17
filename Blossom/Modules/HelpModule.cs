namespace Blossom.Modules;

public sealed class HelpModule : BaseInteractionModule
{
    public HelpModule(IServiceProvider services) : base(services)
    {
    }

    [SlashCommand("commands", "Sends the command list")]
    public async Task CommandsCommand()
    {
        await RespondWithEmbedAsync(
            description: $"{Client.CurrentUser.Mention}'s Commands",
            fields: InteractionService.Modules
                .Select(static (module) => CreateField(
                    $"> {module.Name}",
                    string.Join("\n", module.SlashCommands.Select(static (command) => $"`{command.Name}`: {command.Description}"))
                )
            )
                .ToArray()
        );
    }

    
    [SlashCommand("modmail", "Sends a message to mods")]
    public async Task ModmailCommand()
    {
        Modal modal = new ModalBuilder()
            .WithTitle("Modmail")
            .WithCustomId("modmailModal")
            .AddTextInput("Content", "modmailContent", TextInputStyle.Paragraph, "Type the content here...", 8, 512, true)
            .Build();

        await RespondWithModalAsync(modal);
    }
}
