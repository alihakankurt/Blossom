namespace Blossom;

public static class Extensions
{
    private static readonly Random Randomizer;

    static Extensions()
    {
        Randomizer = new Random();
    }

    public static string CutOff(this string self, int maxLength)
    {
        return (self.Length < maxLength) ? self : $"{self[..(maxLength - 3)]}...";
    }

    public static int Random(this Range self)
    {
        return Randomizer.Next(self.Start.Value, self.End.Value);
    }

    public static T Choose<T>(this ICollection<T> self)
    {
        int index = Randomizer.Next(self.Count);
        return self.ElementAt(index);
    }

    public static SocketRole GetTopRole(this SocketGuildUser self)
    {
        return self.Roles.OrderByDescending((role) => role).First();
    }
}
