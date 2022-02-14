namespace ReactivityDemo.Helpers;

public static class StringHelper
{
    private static readonly Random random = new();

    public static string RandomString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, 7)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}