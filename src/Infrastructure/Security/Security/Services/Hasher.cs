using System.Security.Cryptography;
using System.Text;
using Security.Core.Interfaces;

namespace Security.Services;

public class Hasher : IValueHasher, IPasswordHasher
{
	public string Hash(string value)
		=> Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

	public bool Verify(string value, string hash)
		=> string.Equals(Hash(value), hash, StringComparison.Ordinal);

	public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

	public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
