namespace Main.Modules.Organization.Abstractions;

public interface IOrganizationLocks
{
	Task LockExistingAsync(
		IReadOnlyCollection<Guid> organizationIds,
		CancellationToken cancellationToken = default);
}
