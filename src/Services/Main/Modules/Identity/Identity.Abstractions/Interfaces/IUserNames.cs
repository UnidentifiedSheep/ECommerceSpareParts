namespace Main.Modules.Identity.Abstractions;

public interface IUserNames
{
	Task<IReadOnlyDictionary<Guid, string?>> GetFullNamesAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default);
}
