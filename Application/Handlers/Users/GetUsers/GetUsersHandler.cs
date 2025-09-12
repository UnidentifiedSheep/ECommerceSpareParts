using Application.Interfaces;
using Core.Dtos.Amw.Users;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Mapster;

namespace Application.Handlers.Users.GetUsers;

public record GetUsersQuery(
    PaginationModel Pagination,
    double? SimilarityLevel,
    string? WhoSearchedUserId,
    string? Name,
    string? Surname,
    string? Email,
    string? Phone,
    string? UserName,
    string? Id,
    string? Description,
    bool? IsSupplier,
    GeneralSearchStrategy SearchStrategy) : IQuery<GetUsersResult>;

public record GetUsersResult(IEnumerable<UserDto> Users);

public class GetUsersHandler(IUsersRepository usersRepositoryService) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = new List<AspNetUser>();

        switch (request.SearchStrategy)
        {
            case GeneralSearchStrategy.Exec:
                break;
            case GeneralSearchStrategy.Similarity:
                var simLevel = request.SimilarityLevel >= 1 ? 0.999 : request.SimilarityLevel;
                users.AddRange(await usersRepositoryService.GetUsersBySimilarityAsync(simLevel ?? 0.4,
                    request.Pagination.Page,
                    request.Pagination.Size,
                    request.Name, request.Surname, request.Email, request.Phone, request.UserName, request.Id,
                    request.Description, request.IsSupplier, cancellationToken));
                break;
            case GeneralSearchStrategy.FromStart:
                break;
            case GeneralSearchStrategy.Contains:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new GetUsersResult(users.Adapt<List<UserDto>>());
    }
}