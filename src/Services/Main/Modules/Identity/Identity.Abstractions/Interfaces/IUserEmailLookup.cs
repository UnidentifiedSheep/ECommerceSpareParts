namespace Main.Modules.Identity.Abstractions.Interfaces;

public interface IUserEmailLookup
{
	Task<IReadOnlyDictionary<Guid, string>> GetPrimaryConfirmedEmailsAsync(
		IReadOnlyCollection<Guid> userIds,
		CancellationToken cancellationToken = default);
}
