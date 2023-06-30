namespace Bloom;

public static class Extensions
{
    private static readonly Random Randomizer;

    static Extensions()
    {
        Randomizer = new Random();
    }

    internal static IList<T> Shuffle<T>(this IList<T> self)
    {
        int length = self.Count;
        while (length > 1)
        {
            int index = Randomizer.Next(length--);
            (self[index], self[length]) = (self[length], self[index]);
        }

        return self;
    }
}