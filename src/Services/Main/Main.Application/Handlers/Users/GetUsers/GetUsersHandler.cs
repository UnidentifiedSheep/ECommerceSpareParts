using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Exceptions;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Handlers.Projections;
using Main.Entities.Auth.ValueObjects;
using Main.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using User = Main.Entities.User.User;

namespace Main.Application.Handlers.Users.GetUsers;

public record GetUsersQuery(
    Pagination Pagination,
    string? SearchColumn,
    double? SimilarityLevel,
    Guid? WhoSearchedUserId,
    string? Name,
    string? Surname,
    string? Email,
    string? Phone,
    string? UserName,
    Guid? Id,
    string? Description,
    IEnumerable<string>? Roles,
    GeneralSearchStrategy SearchStrategy) : IQuery<GetUsersResult>;

public record GetUsersResult(IEnumerable<UserDto> Users);

public class GetUsersHandler(IReadRepository<User, Guid> readRepository) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = new List<User>();
        switch (request.SearchStrategy)
        {
            case GeneralSearchStrategy.General:
                return new GetUsersResult(await GeneralSearch(request, cancellationToken));
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

        return new GetUsersResult(users);
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

    private async Task<IReadOnlyList<UserDto>> GeneralSearch(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchColumn))
            throw new InvalidInputException();
        
        var trimmed = request.SearchColumn.Trim();
        return await readRepository.Query
            .Where(x => x.UserInfo != null)
            .ExcludeUsersWithRole(Role.System)
            .IncludeUsersWithRoles(request.Roles ?? [])
            .Select(x => new
            {
                Rank = EF.Functions.TrigramsSimilarity(x.UserInfo!.SearchColumn, trimmed) +
                       EF.Functions
                           .TrigramsWordSimilarity(x.UserInfo!.SearchColumn, trimmed) * 0.7,
                User = x
            })
            .Where(x => x.Rank >= 0.3)
            .OrderByDescending(x => x.Rank)
            .Select(x => x.User)
            .AsExpandable()
            .Select(UserProjections.UserProjection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
    }
}