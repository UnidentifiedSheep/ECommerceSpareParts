using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Exceptions;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Handlers.Projections;
using Main.Entities.User.ValueObjects;
using Main.Enums;
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
        switch (request.SearchStrategy)
        {
            case GeneralSearchStrategy.General:
                return new GetUsersResult(await GeneralSearch(request, cancellationToken));
            case GeneralSearchStrategy.Similarity:
                return new GetUsersResult(await SimilaritySearch(request, cancellationToken));
            case GeneralSearchStrategy.FromStart:
            case GeneralSearchStrategy.Contains:
            case GeneralSearchStrategy.Exec:
            default:
                throw new NotSupportedException();
        }
    }

    private async Task<IReadOnlyList<UserDto>> SimilaritySearch(
        GetUsersQuery request, 
        CancellationToken cancellationToken)
    {
        var simLevel = (request.SimilarityLevel >= 1 ? 0.999 : request.SimilarityLevel) ?? 0.4;
        var query = readRepository.Query
            .Where(x => x.UserInfo != null)
            .ExcludeUsersWithRole(Role.System)
            .IncludeUsersWithRoles(request.Roles ?? []);
        
        var isNameIncluded = false;
        var isSurnameIncluded = false;
        var isEmailIncluded = false;
        var isPhoneNumberIncluded = false;
        var isUserNameIncluded = false;
        var isDescriptionIncluded = false;

        var currName = "";
        var currSurname = "";
        var normalizedUserName = "";
        var normalizedEmail = "";
        var normalizedPhone = "";
        var normalizedDescription = "";

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            currName = request.Name.Trim().ToUpperInvariant();
            query = query.Where(u => EF.Functions.TrigramsSimilarity(u.UserInfo!.Name.ToUpper(),
                                         currName) > simLevel);
            isNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Surname))
        {
            currSurname = request.Surname.Trim().ToUpperInvariant();
            query = query.Where(u => EF.Functions.TrigramsSimilarity(u.UserInfo!.Surname.ToUpper(),
                                         currSurname) > simLevel);
            isSurnameIncluded = true;
        }


        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            normalizedEmail = Email.ToNormalized(request.Email);
            query = query.Where(u => u.Emails.Any(e =>
                EF.Functions.TrigramsSimilarity(e.Email.Value,
                    normalizedEmail) > simLevel));
            isEmailIncluded = true;
        }

        /*if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            normalizedPhone = request.Phone.Trim().ToNormalizedPhoneNumber();
            query = query.Where(u => u.UserPhones.Any(p =>
                EF.Functions.TrigramsSimilarity(p.NormalizedPhone, normalizedPhone) > similarityLevel));
            isPhoneNumberIncluded = true;
        }*/ //TODO phone search currently not supported.

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            normalizedUserName = UserName.ToNormalized(request.UserName);
            query = query.Where(u =>
                EF.Functions.TrigramsSimilarity(u.UserName.NormalizedValue, normalizedUserName) > simLevel);
            isUserNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            normalizedDescription = request.Description.Trim().ToUpperInvariant();
            query = query.Where(u => u.UserInfo!.Description != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Description.ToUpper(),
                                         normalizedDescription) > simLevel);
            isDescriptionIncluded = true;
        }

        if (request.Id != null)
            query = query.Where(u => u.Id == request.Id);

        var queryWithScore = query.Select(u => new
            {
                User = u,
                Score =
                    (isNameIncluded ? EF.Functions.TrigramsSimilarity(u.UserInfo!.Name, currName) : 0) +
                    (isSurnameIncluded ? EF.Functions.TrigramsSimilarity(u.UserInfo!.Surname, currSurname) : 0) +
                    (isUserNameIncluded
                        ? EF.Functions.TrigramsSimilarity(u.UserName.NormalizedValue, normalizedUserName)
                        : 0) +
                    (isEmailIncluded
                        ? u.Emails
                            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.Email.Value, normalizedEmail))
                            .Select(x => EF.Functions.Greatest(
                                EF.Functions.TrigramsSimilarity(x.Email.Value, normalizedEmail)))
                            .FirstOrDefault()
                        : 0) +
                    (isDescriptionIncluded
                        ? EF.Functions.TrigramsSimilarity(u.UserInfo!.Description!, normalizedDescription)
                        : 0) +
                    (isPhoneNumberIncluded
                        ? u.Phones
                            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.NormalizedPhone, normalizedPhone))
                            .Select(x => EF.Functions.Greatest(
                                EF.Functions.TrigramsSimilarity(x.NormalizedPhone, normalizedPhone)))
                            .FirstOrDefault()
                        : 0)
            })
            .OrderByDescending(x => x.Score);

        return await queryWithScore
            .Select(x => x.User)
            .AsExpandable()
            .Select(UserProjections.UserProjection)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<UserDto>> GeneralSearch(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchColumn))
            throw new InvalidInputException("user.search.strategy.not.valid.input");
        
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