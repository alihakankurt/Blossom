namespace Blossom.Modules;

[Name("Owner Module")]
public sealed class OwnerModule : ModuleBase
{
    public OwnerModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Command("add-module"), Summary("Adds a module")]
    [RequireOwner]
    public async Task AddModuleCommand([Summary("The name of the module"), Remainder] string name)
    {
        Type module = Type.GetType($"Blossom.Modules.{name}", false, true);

        if (module == null)
        {
            await ReplyAsync("Module could not be found!");
            return;
        }

        try
        {
            await CommandService.AddModuleAsync(module, ServiceProvider);
            await ReplyAsync("Module successfully added.");
        }
        catch (ArgumentException)
        {
            await ReplyAsync("This module has already been added!");
        }
        catch (InvalidOperationException)
        {
            await ReplyAsync("Module could not be added!");
        }
    }

    [Command("remove-module"), Summary("Removes a module")]
    [RequireOwner]
    public async Task RemoveModuleCommand([Summary("The name of the module"), Remainder] string name)
    {
        Type module = Type.GetType($"Blossom.Modules.{name}", false, true);

        if (module == null)
        {
            await ReplyAsync("Module could not be found!");
            return;
        }

        var result = await CommandService.RemoveModuleAsync(module);
        await ReplyAsync(result ? "Module successfully removed." : "This module has not been added yet!");
    }
}
