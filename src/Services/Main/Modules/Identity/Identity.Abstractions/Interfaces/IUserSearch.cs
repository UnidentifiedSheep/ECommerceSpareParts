namespace Main.Modules.Identity.Abstractions.Interfaces;

public interface IUserSearch
{
	IQueryable<Guid> MatchingIds(string term);

	IQueryable<UserMatch> MatchWithRank(string term);
}

public sealed record UserMatch
{
	public required Guid UserId { get; init; }

	public required float Rank { get; init; }
}
