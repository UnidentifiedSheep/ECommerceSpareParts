namespace Main.Modules.Organization.Abstractions;

public interface IOrganizationSearch
{
	IQueryable<Guid> MatchingIds(string term);
}
