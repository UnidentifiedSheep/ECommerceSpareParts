namespace Main.Modules.Identity.Abstractions;

public sealed record UserMatch(Guid UserId, float Rank);

public interface IUserLookup
{
	Task EnsureExistAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<UserMatch>> SearchAsync(
		string searchTerm,
		int limit,
		CancellationToken cancellationToken = default);
}
