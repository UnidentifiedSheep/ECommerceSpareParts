namespace Security.Core.Interfaces;

public interface IValueHasher
{
	string Hash(string value);

	bool Verify(string value, string hash);
}
