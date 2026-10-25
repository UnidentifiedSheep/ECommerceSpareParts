using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Role = Enums.Role;

namespace Main.Application.Handlers.Users.GetUsers;

public record GetUsersQuery(
	Pagination Pagination,
	string? Name,
	string? Surname,
	string? Email,
	string? Phone,
	string? UserName,
	Guid? Id,
	string? Description,
	IReadOnlyCollection<string>? Roles) : IQuery<GetUsersResult>;

public record GetUsersResult(IReadOnlyList<UserDto> Users);

public class GetUsersHandler(
	IReadRepository<User, Guid> readRepository,
	IProjectionProvider<User, UserDto> projection) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
	public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
	{
		var query = readRepository.Query
			.ExcludeUsersWithRole(Role.System)
			.IncludeUsersWithRoles(request.Roles ?? []);

		if (request.Id.HasValue)
			query = query.Where(user => user.Id == request.Id.Value);

		if (!string.IsNullOrWhiteSpace(request.Name))
		{
			var pattern = EscapePattern(request.Name.Trim()) + "%";
			query = query.Where(user => user.UserInfo != null &&
				EF.Functions.ILike(user.UserInfo.Name, pattern, "\\"));
		}

		if (!string.IsNullOrWhiteSpace(request.Surname))
		{
			var pattern = EscapePattern(request.Surname.Trim()) + "%";
			query = query.Where(user => user.UserInfo != null &&
				EF.Functions.ILike(user.UserInfo.Surname, pattern, "\\"));
		}

		if (!string.IsNullOrWhiteSpace(request.UserName))
		{
			var normalized = UserName.ToNormalized(request.UserName);
			query = query.Where(user => user.UserName.NormalizedValue.StartsWith(normalized));
		}

		if (!string.IsNullOrWhiteSpace(request.Email))
		{
			Email email = request.Email;
			query = query.Where(user => user.Emails.Any(contact => contact.Email == email));
		}

		if (!string.IsNullOrWhiteSpace(request.Phone))
		{
			var normalized = UserPhone.ToNormalizedPhone(request.Phone);
			query = query.Where(user => user.Phones.Any(contact =>
				contact.NormalizedPhone.StartsWith(normalized)));
		}

		if (!string.IsNullOrWhiteSpace(request.Description))
		{
			var pattern = "%" + EscapePattern(request.Description.Trim()) + "%";
			query = query.Where(user => user.UserInfo != null &&
				user.UserInfo.Description != null &&
				EF.Functions.ILike(user.UserInfo.Description, pattern, "\\"));
		}

		var users = await query
			.OrderBy(user => user.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetUsersResult(users);
	}

	private static string EscapePattern(string value) =>
		value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
