namespace Blossom.Modules;

[EnabledInDm(true)]
public sealed class FunModule : BaseInteractionModule
{
    private const string UwuUrl = "https://www.youtube.com/watch?v=PqIMNE7QBSQ";

    private static readonly string[] CoinSides;
    private static readonly string[] UwuFaces;
    private static readonly string[] Fruits;

    private readonly AudioService _audioService;
    private LavaTrack? _uwuTrack;

    static FunModule()
    {
        CoinSides = new[] { "Heads", "Tails" };
        UwuFaces = new[] { "uwu", "UwU", "(・`ω´・)", ";;w;;", ">w<", "^w^", ">.<", "(ᵘﻌᵘ)", "ᵾwᵾ", "𝕌𝕨𝕌", "𝓤𝔀𝓤", "( ｡ᵘ ᵕ ᵘ ｡)", "( ᵘ ꒳ ᵘ ✼)", "(⁄˘⁄ ⁄ ω⁄ ⁄ ˘⁄)♡", "✧･ﾟ: *✧･ﾟ♡*(ᵘʷᵘ)*♡･ﾟ✧*:･ﾟ✧" };
        Fruits = new[] { ":cherries:", ":tangerine:", ":watermelon:", ":lemon:", ":peach:", ":grapes:", ":pineapple:", ":blueberries:" };
    }

    public FunModule(IServiceProvider services, AudioService audioService) : base(services)
    {
        _audioService = audioService;
    }

    [SlashCommand("hi", "hi")]
    public async Task HiCommand()
    {
        await RespondAsync($"Hi, `{User.Username}`. How are you?");
    }

    [SlashCommand("flip", "Flips a coin")]
    public async Task FlipCommand()
    {
        await RespondWithEmbedAsync($":coin: {CoinSides.Choose()}");
    }

    [SlashCommand("roll", "Rolls a dice")]
    public async Task RollCommand()
    {
        await RespondWithEmbedAsync($":game_die: {(1..6).Random()}");
    }

    [SlashCommand("uwu", "uwu")]
    public async Task UwuCommand()
    {
        if (Channel is not IDMChannel && (0..99).Random() < 4)
        {
            LavaPlayer? player = _audioService.GetPlayer(Guild);
            IVoiceState? voiceState = User as IVoiceState;
            if (player is null && voiceState?.VoiceChannel is not null)
            {
                player = await _audioService.JoinAsync(voiceState.VoiceChannel, (ITextChannel)Channel);
                _uwuTrack ??= (await _audioService.SearchAsync(UwuUrl)).Tracks.First();
                await player.PlayAsync(_uwuTrack);
                await Task.Delay(3000);
                await _audioService.LeaveAsync(voiceState.VoiceChannel);
            }
        }

        await RespondAsync(UwuFaces.Choose());
    }

    [SlashCommand("slot", "Runs slot machine")]
    public async Task SlotCommand()
    {
        string a = Fruits.Choose();
        string b = Fruits.Choose();
        string c = Fruits.Choose();

        StringBuilder result = new($"[ {a} {b} {c} ] ");

        if (a == b && a == c)
        {
            _ = result.Append((a == Fruits[0]) ? "Cherries, yum!!" : "Congrats, you can eat them all.");
        }
        else if (a == b || a == c || b == c)
        {
            _ = result.Append("Congrats, two of them matches.");
        }
        else
        {
            _ = result.Append("Sorry, no matches.");
        }

        await RespondAsync(result.ToString());
    }
}
