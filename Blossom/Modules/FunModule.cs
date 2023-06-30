namespace Blossom.Modules;

public sealed class FunModule : BaseInteractionModule
{
    private static readonly string[] CoinSides;
    private static readonly string[] UwuFaces;
    private static readonly string[] Fruits;

    static FunModule()
    {
        CoinSides = new[] { "Heads", "Tails" };
        UwuFaces = new[] { "uwu", "UwU", "(・`ω´・)", ";;w;;", ">w<", "^w^", ">.<", "(ᵘﻌᵘ)", "ᵾwᵾ", "𝕌𝕨𝕌", "𝓤𝔀𝓤", "( ｡ᵘ ᵕ ᵘ ｡)", "( ᵘ ꒳ ᵘ ✼)", "(⁄˘⁄ ⁄ ω⁄ ⁄ ˘⁄)♡", "✧･ﾟ: *✧･ﾟ♡*(ᵘʷᵘ)*♡･ﾟ✧*:･ﾟ✧" };
        Fruits = new[] { ":cherries:", ":tangerine:", ":watermelon:", ":lemon:", ":peach:", ":grapes:", ":pineapple:", ":blueberries:" };
    }

    public FunModule(IServiceProvider services) : base(services)
    {
    }

    [SlashCommand("hi", "hi")]
    public async Task HiCommand()
    {
        await RespondAsync($"Hi, `{User.GlobalName}`. How are you?");
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

    [SlashCommand("uwuify", "Sends your message back in a cute way.")]
    public async Task UwuCommand([Summary(description: "The text to uwuify >.<"), MaxValue(512)] string text)
    {
        var result = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] is 'L' or 'R')
                result.Append('W');
            else if (text[i] is 'l' or 'r')
                result.Append('w');
            else if (text[i] is 'o' or 'O' && i > 0 && text[i - 1] is 'N' or 'M' or 'n' or 'm')
                result.Append(text[i] is 'o' ? "yo" : "YO");
            else
                result.Append(text[i]);
        }

        result.Append(' ');
        result.Append(UwuFaces.Choose());
        await RespondAsync(result.ToString());
    }

    [SlashCommand("slot", "Runs slot machine")]
    public async Task SlotCommand()
    {
        string a = Fruits.Choose();
        string b = Fruits.Choose();
        string c = Fruits.Choose();

        StringBuilder result = new($"[ {a} {b} {c} ] ");

        if (a == b && a == c)
            result.Append((a == Fruits[0]) ? "Cherries, yum!!" : "Congrats, you can taste them all.");
        else if (a == b || a == c || b == c)
            result.Append("Congrats, two of them matches.");
        else
            result.Append("Sorry, no matches.");

        await RespondAsync(result.ToString());
    }

    [SlashCommand("joke", "Makes a joke")]
    public async Task JokeCommand()
    {
        var joke = await SomeRandomApi.GetJokeAsync();
        await RespondAsync(joke);
    }

    [SlashCommand("quote", "Says a quote from animes")]
    public async Task QuoteCommand()
    {
        var quote = await SomeRandomApi.GetQuoteAsync();
        await RespondAsync(quote);
    }
}
