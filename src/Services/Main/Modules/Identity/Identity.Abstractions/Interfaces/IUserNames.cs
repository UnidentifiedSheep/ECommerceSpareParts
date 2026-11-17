namespace Main.Modules.Identity.Abstractions.Interfaces;

public interface IUserNames
{
	Task<IReadOnlyDictionary<Guid, string?>> GetFullNamesAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default);
}
