using Abstractions.Models;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Dtos.RepositoryOptionsData;
using Main.Enums;
using Mapster;
using User = Main.Entities.User.User;

namespace Main.Application.Handlers.Users.GetUsers;

public record GetUsersQuery(
    string? SearchTerm,
    Pagination Pagination,
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
        switch (request.SearchStrategy)
        {
            case GeneralSearchStrategy.General:
                users.AddRange(await GeneralSearch(request, cancellationToken));
                break;
            case GeneralSearchStrategy.Exec:
                break;
            case GeneralSearchStrategy.Similarity:
                users.AddRange(await SimilaritySearch(request, cancellationToken));
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

    private async Task<IReadOnlyList<User>> SimilaritySearch(
        GetUsersQuery request, 
        CancellationToken cancellationToken)
    {
        var simLevel = (request.SimilarityLevel >= 1 ? 0.999 : request.SimilarityLevel) ?? 0.4;
        var options = new QueryOptions<User,GetUsersBySimilarityOptionsData>
        {
            Data = new GetUsersBySimilarityOptionsData
            {
                Description = request.Description,
                IsSupplier = request.IsSupplier,
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Phone = request.Phone,
                UserName = request.UserName,
                Id = request.Id,
                SimilarityLevel = simLevel
            }
        }.WithTracking(false)
        .WithInclude(x => x.UserInfo)
        .WithPage(request.Pagination.Page)
        .WithSize(request.Pagination.Size);

        return await usersRepositoryService.GetUsersBySimilarityAsync(options, cancellationToken);
    }

    private async Task<IReadOnlyList<User>> GeneralSearch(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var options = new QueryOptions<User, GetUserBySearchColumnOptionsData>()
            {
                Data = new GetUserBySearchColumnOptionsData
                {
                    SearchTerm = request.SearchTerm,
                    IsSupplier = request.IsSupplier,
                }
            }
            .WithTracking(false)
            .WithInclude(x => x.UserInfo)
            .WithPage(request.Pagination.Page)
            .WithSize(request.Pagination.Size);

        return await usersRepositoryService.GetUserBySearchColumn(
            options,
            cancellationToken);
    }
}