using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Role = Enums.Role;

namespace Main.Application.Handlers.Users.SearchUsers;

public sealed record SearchUsersQuery(
	string SearchTerm,
	IReadOnlyCollection<string>? Roles,
	Pagination Pagination) : IQuery<SearchUsersResult>;

public sealed record SearchUsersResult(IReadOnlyList<UserDto> Users);

public sealed class SearchUsersHandler(
	IUserRepository userRepository,
	IReadRepository<User, Guid> readRepository,
	IProjectionProvider<User, UserDto> projection)
	: IQueryHandler<SearchUsersQuery, SearchUsersResult>
{
	public async Task<SearchUsersResult> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
	{
		var users = await userRepository
			.Search(readRepository.Query, request.SearchTerm)
			.ExcludeUsersWithRole(Role.System)
			.IncludeUsersWithRoles(request.Roles ?? [])
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new SearchUsersResult(users);
	}
}
