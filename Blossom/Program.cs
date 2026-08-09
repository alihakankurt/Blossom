using System.Runtime.InteropServices;
using Blossom;

using var cts = new CancellationTokenSource();

void OnSignal(PosixSignalContext context)
{
    context.Cancel = true;
    cts.Cancel();
}

using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, OnSignal);
using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, OnSignal);

var bot = new Bot();
await bot.RunAsync(cts.Token);
