using Application.Common.Interfaces;
using Core.Models;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Enums;
using Mapster;

using User = Main.Entities.User;

namespace Main.Application.Handlers.Users.GetUsers;

public record GetUsersQuery(
    string? SearchTerm,
    PaginationModel Pagination,
    double? SimilarityLevel,
    Guid? WhoSearchedUserId,
    string? Name,
    string? Surname,
    string? Email,
    string? Phone,
    string? UserName,
    Guid? Id,
    string? Description,
    bool? IsSupplier,
    GeneralSearchStrategy SearchStrategy) : IQuery<GetUsersResult>;

public record GetUsersResult(IEnumerable<UserDto> Users);

public class GetUsersHandler(IUserRepository usersRepositoryService) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = new List<User>();
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        switch (request.SearchStrategy)
        {
            case GeneralSearchStrategy.General:
                users.AddRange(await usersRepositoryService.GetUserBySearchColumn(request.SearchTerm, page,
                    size, request.IsSupplier, false, cancellationToken));
                break;
            case GeneralSearchStrategy.Exec:
                break;
            case GeneralSearchStrategy.Similarity:
                var simLevel = request.SimilarityLevel >= 1 ? 0.999 : request.SimilarityLevel;
                users.AddRange(await usersRepositoryService.GetUsersBySimilarityAsync(simLevel ?? 0.4,
                    page, size,
                    request.Name, request.Surname, request.Email, request.Phone, request.UserName, request.Id,
                    request.Description, request.IsSupplier, false, cancellationToken));
                break;
            case GeneralSearchStrategy.FromStart:
            case GeneralSearchStrategy.Contains:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var systemUser = users.FirstOrDefault(x => x.Id == Global.SystemId);
        if (systemUser != null) users.Remove(systemUser);

        return new GetUsersResult(users.Adapt<List<UserDto>>());
    }
}