namespace Blossom.Modules;

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
        await RespondWithEmbedAsync($":game_die: {Extensions.Random(1, 7)}");
    }

    [SlashCommand("luck", "Checks your luck")]
    public async Task LuckCommand()
    {
        float luck = Extensions.Random(0, 101);
        string message = $"Your luck ratio is `{luck / 100.0:n2}`. ";
        if (luck < 1)
        {
            message += "I suggest you, go kill yourself. This is the painless way for you. There is no hope for you.";
        }
        else if (luck < 20)
        {
            message += "You can live your poor life.";
        }
        else if (luck < 50)
        {
            message += "You have a bad luck but still there is a litte bit of hope.";
        }
        else if (luck < 70)
        {
            message += "You are a average person with possibilities. Nothing special.";
        }
        else if (luck < 100)
        {
            message += "You have a life to live dude.";
        }
        else
        {
            message += "You are the luckiest person in the world!";
        }

        await RespondAsync(message);
    }

    [SlashCommand("uwu", "uwu")]
    public async Task UwuCommand()
    {
        if (Channel is not IDMChannel && Extensions.Random(0, 101) < 4)
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
            _ = result.Append(
                (a == Fruits[0]) 
                    ? "Cherries, yum!!" 
                    : "Congrats, you can taste them all."
            );
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
