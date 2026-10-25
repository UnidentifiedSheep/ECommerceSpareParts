using Main.Entities.User;
using NpgsqlTypes;

namespace Main.Persistence.ReadModels;

internal sealed class UserSearchDocument
{
	public Guid UserId { get; private set; }
	public NpgsqlTsVector SearchVector { get; private set; } = null!;
	public User User { get; private set; } = null!;
}
