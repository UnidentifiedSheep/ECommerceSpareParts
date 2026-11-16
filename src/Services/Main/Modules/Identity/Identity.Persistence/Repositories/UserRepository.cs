using Application.Common.Interfaces.Repositories;
using Main.Entities.Auth;
using Main.Entities.Enums;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Modules.Identity.Application.Interfaces.Persistence;
using Main.Modules.Identity.Application.Models.Users;
using Main.Modules.Identity.Persistence.DatabaseObjects;
using Main.Modules.Identity.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Modules.Identity.Persistence.Repositories;

public class UserRepository(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, User, Guid>(context, extensions), IUserRepository
{
	public async Task RevokeRefreshTokensAsync(
		Guid userId,
		string reason,
		CancellationToken cancellationToken = default)
		=> await Context.Set<UserToken>()
			.Where(token => token.UserId == userId && token.Type == TokenType.RefreshToken && !token.Revoked)
			.ExecuteUpdateAsync(
				setters => setters.SetProperty(token => token.Revoked, true)
					.SetProperty(token => token.RevokeReason, reason),
				cancellationToken);

	public IQueryable<User> Search(
		IQueryable<User> query,
		string searchTerm)
		=> SearchWithRank(query, searchTerm)
			.Select(match => match.User);

	public IQueryable<UserSearchMatch> SearchWithRank(
		IQueryable<User> query,
		string searchTerm)
	{
		var normalized = searchTerm.Trim();
		if (normalized.Any(char.IsDigit) &&
			normalized.All(character => char.IsDigit(character) || char.IsWhiteSpace(character) ||
				character is '+' or '-' or '(' or ')'))
			normalized = UserPhone.ToNormalizedPhone(normalized);

		return query.AsNoTracking()
			.Join(
				Context.Set<UserSearchDocument>(),
				user => user.Id,
				document => document.UserId,
				(user, document) => new { User = user, Document = document })
			.Where(match
				=> match.Document.SearchVector.Matches(UserSearchDbFunctions.ToUserSearchQuery(normalized)))
			.Select(match => new UserSearchMatch
			{
				User = match.User,
				Rank = match.Document.SearchVector.Rank(UserSearchDbFunctions.ToUserSearchQuery(normalized))
			})
			.OrderByDescending(match => match.Rank)
			.ThenBy(match => match.User.Id);
	}

	public Task<User?> GetUserByPrimaryEmailAsync(
		string email,
		Criteria<User>? criteria = null,
		CancellationToken cancellationToken = default)
	{
		Email normalizedEmail = email;
		var query = Context.Set<User>()
			.Where(x => x.Emails.Any(z => z.Email == normalizedEmail && z.IsPrimary));

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return query.FirstOrDefaultAsync(cancellationToken);
	}

	public Task<User?> GetUserByLoginAsync(
		string login,
		bool isEmail,
		Criteria<User>? criteria = null,
		CancellationToken cancellationToken = default)
	{
		if (isEmail)
			return GetUserByPrimaryEmailAsync(
				login,
				criteria,
				cancellationToken);

		var normalizedUserName = UserName.ToNormalized(login);
		var query = Context.Set<User>()
			.Where(x => x.UserName.NormalizedValue == normalizedUserName);

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return query.FirstOrDefaultAsync(cancellationToken);
	}
}
