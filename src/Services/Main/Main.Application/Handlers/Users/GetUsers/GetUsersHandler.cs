using Abstractions.Models;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
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
        var options = new PageableQueryOptions<User>()
            .WithTracking(false)
            .WithInclude(x => x.UserInfo)
            .WithPage(request.Pagination.Page)
            .WithSize(request.Pagination.Size);
        var users = new List<User>();
        switch (request.SearchStrategy)
        {
            case GeneralSearchStrategy.General:
                users.AddRange(await usersRepositoryService.GetUserBySearchColumn(
                    request.SearchTerm, 
                    request.IsSupplier, 
                    options, 
                    cancellationToken));
                break;
            case GeneralSearchStrategy.Exec:
                break;
            case GeneralSearchStrategy.Similarity:
                var simLevel = request.SimilarityLevel >= 1 ? 0.999 : request.SimilarityLevel;
                users.AddRange(await usersRepositoryService.GetUsersBySimilarityAsync(
                    simLevel ?? 0.4,
                    options,
                    request.Name, request.Surname, request.Email, request.Phone, request.UserName, request.Id,
                    request.Description, request.IsSupplier, cancellationToken));
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