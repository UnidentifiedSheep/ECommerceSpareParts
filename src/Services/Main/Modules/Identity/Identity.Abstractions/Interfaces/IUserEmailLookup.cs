namespace Main.Modules.Identity.Abstractions;

public interface IUserEmailLookup
{
	Task<IReadOnlyDictionary<Guid, string>> GetPrimaryConfirmedEmailsAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default);
}
