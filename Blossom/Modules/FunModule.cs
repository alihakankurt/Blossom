namespace Blossom.Modules;

[Name("Fun Module")]
public class FunModule : ModuleBase
{
    private const string hannahUwuUrl = "https://www.youtube.com/watch?v=PqIMNE7QBSQ";
    private static readonly string[] uwuFaces = { "uwu", "UwU", "(・`ω´・)", ";;w;;", ">w<", "^w^", ">.<", "(ᵘﻌᵘ)", "ᵾwᵾ", "𝕌𝕨𝕌", "𝓤𝔀𝓤", "( ｡ᵘ ᵕ ᵘ ｡)", "( ᵘ ꒳ ᵘ ✼)", "(⁄˘⁄ ⁄ ω⁄ ⁄ ˘⁄)♡", "✧･ﾟ: *✧･ﾟ♡*(ᵘʷᵘ)*♡･ﾟ✧*:･ﾟ✧" };

    private static readonly string[] fruits = { "🍒", "🍊", "🍉", "🍋", "🍑", "🍇", "🍍", "🫐" };

    private readonly LavaNode lavaNode;
    private LavaTrack uwuTrack;

    public FunModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        lavaNode = serviceProvider.GetService<LavaNode>();
    }

    [Command("hi"), Summary("Hii")]
    public async Task HiCommand()
    {
        await ReplyAsync($"Hii {User.Username}!");
    }

    [Command("uwu"), Summary("uwu")]
    public async Task UwuCommand()
    {
        if (RandomNumber(0, 99) < 4)
        {
            IVoiceState voiceState = User as IVoiceState;

            if (!lavaNode.HasPlayer(Guild) && voiceState?.VoiceChannel != null)
            {
                LavaPlayer player = await lavaNode.JoinAsync(voiceState.VoiceChannel, Channel as ITextChannel);

                if (uwuTrack == null)
                {
                    SearchResponse response = await lavaNode.SearchAsync(SearchType.YouTube, hannahUwuUrl);
                    uwuTrack = response.Tracks.First();
                }

                await player.PlayAsync(uwuTrack);
                await Task.Delay(3000);
                await lavaNode.LeaveAsync(voiceState.VoiceChannel);
                return;
            }
        }

        await ReplyAsync(Choose<string>(uwuFaces));
    }

    [Command("flip"), Summary("Flips a coin")]
    public async Task FlipCommand()
    {
        await ReplyWithEmbedAsync($"🪙 {Choose<string>("Heads", "Tails")}");
    }

    [Command("roll"), Summary("Rolls a dice")]
    public async Task RollCommand()
    {
        await ReplyWithEmbedAsync($"🎲 {RandomNumber(1, 6)}");
    }

    [Command("slot"), Summary("Runs slot machine")]
    public async Task SlotCommand()
    {
        string a = Choose<string>(fruits);
        string b = Choose<string>(fruits);
        string c = Choose<string>(fruits);

        StringBuilder result = new($"~~[ {a} {b} {c} ]~~ ");

        if (a == b && a == c)
        {
            result.Append((a == fruits[0]) ? "Cherries, yum!!" : "Congrats, you can eat them all.");
        }
        else if (a == b || a == c || b == c)
        {
            result.Append("Congrats, two of them matches.");
        }
        else
        {
            result.Append("Sorry, no matches.");
        }

        await ReplyAsync(result.ToString());
    }
}
