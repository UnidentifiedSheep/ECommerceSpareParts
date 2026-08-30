using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public record GetUserFullInfoQuery(Guid UserId) : IQuery<GetUserFullInfoResult>;

public record GetUserFullInfoResult(
	UserDto User,
	IReadOnlyList<UserEmailDto> Emails,
	IReadOnlyList<UserPhoneDto> Phones,
	IReadOnlyList<string> Roles,
	IReadOnlyList<string> Permissions);

public class GetUserFullInfoHandler(
	IReadRepository<User, Guid> repository,
	IUserCacheRepository userCache,
	IProjectionProvider<User, UserDto> userProjection,
	IProjectionProvider<UserEmail, UserEmailDto> emailProjection,
	IProjectionProvider<UserPhone, UserPhoneDto> phoneProjection)
	: IQueryHandler<GetUserFullInfoQuery, GetUserFullInfoResult>
{
	public async Task<GetUserFullInfoResult> Handle(
		GetUserFullInfoQuery request,
		CancellationToken cancellationToken)
	{
		var userToDto = userProjection.Projection;
		var emailToDto = emailProjection.Projection;
		var phoneToDto = phoneProjection.Projection;

		var user = await repository
			.Query
			.Where(x => x.Id == request.UserId)
			.AsExpandable()
			.Select(x => new
			{
				User = userToDto.Invoke(x),
				Emails = x.Emails.Select(z => emailToDto.Invoke(z)),
				Phones = x.Phones.Select(z => phoneToDto.Invoke(z))
			})
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UserNotFoundException(request.UserId);

		var (roles, permissions) =
			await userCache.GetUserRolesAndPermissionsAsync(request.UserId, cancellationToken) ??
			throw new UserNotFoundException(request.UserId);

		return new GetUserFullInfoResult(
			user.User,
			user.Emails.ToList(),
			user.Phones.ToList(),
			roles,
			permissions);
	}
}
