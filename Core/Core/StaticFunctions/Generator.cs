namespace Core.StaticFunctions;

public static class Generator
{
    public static string GeneratePassword(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ =>
            {
                char c = chars[Random.Shared.Next(chars.Length)];
                return char.IsLetter(c) && Random.Shared.Next(2) == 0 ? char.ToUpper(c) : c;
            })
            .ToArray());
    }
}