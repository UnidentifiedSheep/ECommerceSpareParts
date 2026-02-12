using System.Security.Cryptography;
using System.Text;
using Abstractions.Interfaces;

namespace Security.Services;

public class TokenHasher : ITokenHasher
{
    public string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public bool VerifyToken(string token, string hash)
    {
        return HashToken(token) == hash;
    }
}