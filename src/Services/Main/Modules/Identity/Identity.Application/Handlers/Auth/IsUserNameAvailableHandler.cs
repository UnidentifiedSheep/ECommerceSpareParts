using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth;

public record IsUserNameAvailableQuery(string UserName) : IQuery<IsUserNameAvailableResult>;

public record IsUserNameAvailableResult(bool IsAvailable);

public class IsUserNameAvailableHandler(IReadRepository<User, Guid> repository)
	: IQueryHandler<IsUserNameAvailableQuery, IsUserNameAvailableResult>
{
	public async Task<IsUserNameAvailableResult> Handle(
		IsUserNameAvailableQuery request,
		CancellationToken cancellationToken)
	{
		var any = await repository.Query.AnyAsync(
			x => x.UserName.NormalizedValue == UserName.ToNormalized(request.UserName),
			cancellationToken);

		return new IsUserNameAvailableResult(!any);
	}
}
