using Blossom;

var bot = new Bot();

Console.CancelKeyPress += async (_, args) =>
{
    Console.WriteLine("Interrupted. Stopping bot...");
    args.Cancel = false;
    await bot.StopAsync();
};

await bot.RunAsync();
