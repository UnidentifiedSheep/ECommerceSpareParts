using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models.Users;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Persistence.Context;
using Main.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.User;

public class UserRepository(DContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<DContext, Entities.User.User, Guid>(context, extensions), IUserRepository
{
	public IQueryable<Entities.User.User> Search(
		IQueryable<Entities.User.User> query,
		string searchTerm) =>
		SearchWithRank(query, searchTerm)
			.Select(match => match.User);

	public IQueryable<UserSearchMatch> SearchWithRank(
		IQueryable<Entities.User.User> query,
		string searchTerm)
	{
		var normalized = searchTerm.Trim();
		if (normalized.Any(char.IsDigit) &&
			normalized.All(character => char.IsDigit(character) || char.IsWhiteSpace(character) ||
				character is '+' or '-' or '(' or ')'))
			normalized = UserPhone.ToNormalizedPhone(normalized);

		return query.AsNoTracking()
			.Join(Context.Set<UserSearchDocument>(), user => user.Id, document => document.UserId,
				(user, document) => new { User = user, Document = document })
			.Where(match => match.Document.SearchVector.Matches(DContext.ToUserSearchQuery(normalized)))
			.Select(match => new UserSearchMatch
			{
				User = match.User,
				Rank = match.Document.SearchVector.Rank(DContext.ToUserSearchQuery(normalized))
			})
			.OrderByDescending(match => match.Rank)
			.ThenBy(match => match.User.Id);
	}

	public Task<Entities.User.User?> GetUserByPrimaryEmailAsync(
		string email,
		Criteria<Entities.User.User>? criteria = null,
		CancellationToken cancellationToken = default)
	{
		Email normalizedEmail = email;
		var query = Context.Users.Where(x => x.Emails.Any(z => z.Email == normalizedEmail && z.IsPrimary));

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return query.FirstOrDefaultAsync(cancellationToken);
	}

	public Task<Entities.User.User?> GetUserByLoginAsync(
		string login,
		bool isEmail,
		Criteria<Entities.User.User>? criteria = null,
		CancellationToken cancellationToken = default)
	{
		if (isEmail)
			return GetUserByPrimaryEmailAsync(
				login,
				criteria,
				cancellationToken);

		var normalizedUserName = UserName.ToNormalized(login);
		var query = Context.Users.Where(x => x.UserName.NormalizedValue == normalizedUserName);

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return query.FirstOrDefaultAsync(cancellationToken);
	}
}
