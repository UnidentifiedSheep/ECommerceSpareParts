namespace Core.Interfaces;

public interface ITokenHasher
{
    string HashToken(string token);
    bool VerifyToken(string token, string hash);
}