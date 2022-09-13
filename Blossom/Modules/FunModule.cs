namespace Blossom.Modules;

[EnabledInDm(isEnabled: true)]
public sealed class FunModule : InteractionModuleBase
{
    public const string UwuUrl = "https://www.youtube.com/watch?v=PqIMNE7QBSQ";
    public static readonly string[] UwuFaces = { "uwu", "UwU", "(・`ω´・)", ";;w;;", ">w<", "^w^", ">.<", "(ᵘﻌᵘ)", "ᵾwᵾ", "𝕌𝕨𝕌", "𝓤𝔀𝓤", "( ｡ᵘ ᵕ ᵘ ｡)", "( ᵘ ꒳ ᵘ ✼)", "(⁄˘⁄ ⁄ ω⁄ ⁄ ˘⁄)♡", "✧･ﾟ: *✧･ﾟ♡*(ᵘʷᵘ)*♡･ﾟ✧*:･ﾟ✧" };

    public static readonly string[] Fruits = { "🍒", "🍊", "🍉", "🍋", "🍑", "🍇", "🍍", "🫐" };

    private LavaTrack _uwuTrack;

    public FunModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("hi", "hi")]
    public async Task HiCommand()
    {
        await RespondAsync($"Hi, `{User.Username}`. How are you?");
    }

    [SlashCommand("uwu", "uwu")]
    public async Task UwuCommand()
    {
        if (Channel is not IDMChannel && RandomNumber(0, 99) < 4)
        {
            LavaPlayer player = AudioService.GetPlayer(Guild);
            IVoiceState voiceState = User as IVoiceState;
            if (player is null && voiceState?.VoiceChannel != null)
            {
                player = await AudioService.JoinAsync(voiceState.VoiceChannel, Channel as ITextChannel);

                if (_uwuTrack == null)
                {
                    SearchResponse response = await AudioService.SearchAsync(UwuUrl);
                    _uwuTrack = response.Tracks.First();
                }

                await player.PlayAsync(_uwuTrack);
                await Task.Delay(3000);
                await AudioService.LeaveAsync(voiceState.VoiceChannel);
                return;
            }
        }

        await RespondAsync(Choose<string>(UwuFaces));
    }

    [SlashCommand("flip", "Flips a coin")]
    public async Task FlipCommand()
    {
        await RespondWithEmbedAsync($"🪙 {Choose<string>("Heads", "Tails")}");
    }

    [SlashCommand("roll", "Rolls a dice")]
    public async Task RollCommand()
    {
        await RespondWithEmbedAsync($"🎲 {RandomNumber(1, 6)}");
    }

    [SlashCommand("slot", "Runs slot machine")]
    public async Task SlotCommand()
    {
        string a = Choose<string>(Fruits);
        string b = Choose<string>(Fruits);
        string c = Choose<string>(Fruits);

        StringBuilder result = new($"~~[ {a} {b} {c} ]~~ ");

        if (a == b && a == c)
        {
            result.Append((a == Fruits[0]) ? "Cherries, yum!!" : "Congrats, you can eat them all.");
        }
        else if (a == b || a == c || b == c)
        {
            result.Append("Congrats, two of them matches.");
        }
        else
        {
            result.Append("Sorry, no matches.");
        }

        await RespondAsync(result.ToString());
    }
}
