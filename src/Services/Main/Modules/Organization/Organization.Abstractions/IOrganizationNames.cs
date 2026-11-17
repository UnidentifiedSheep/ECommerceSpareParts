namespace Main.Modules.Organization.Abstractions;

public interface IOrganizationNames
{
	Task<IReadOnlyDictionary<Guid, string>> GetAsync(
		IReadOnlyCollection<Guid> organizationIds,
		CancellationToken cancellationToken = default);
}
