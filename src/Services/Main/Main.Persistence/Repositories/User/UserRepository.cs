using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.User.ValueObjects;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.User;

public class UserRepository(DContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<DContext, Entities.User.User, Guid>(context, extensions), IUserRepository
{

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
