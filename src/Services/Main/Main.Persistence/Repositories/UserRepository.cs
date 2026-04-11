using System.Diagnostics.CodeAnalysis;
using Abstractions.Models.Repository;
using Extensions;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.User;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserRepository(DContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(
        QueryOptions<User, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.Users
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.Id == options.Data, cancellationToken);
    }

    public async Task ChangeUsersDiscount(
        Guid userId,
        decimal discount,
        CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlAsync($"""
                                                INSERT INTO user_discounts (user_id, discount)
                                                VALUES ({userId}, {discount})
                                                ON CONFLICT (user_id)
                                                DO UPDATE SET discount = EXCLUDED.discount;
                                                """, cancellationToken);
    }


    public async Task<UserInfo?> GetUserInfo(
        QueryOptions<UserInfo, Guid> options, 
        CancellationToken cancellationToken = default)
    {
        return await context.UserInfos
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.UserId == options.Data, cancellationToken);
    }


    public async Task<IReadOnlyList<User>> GetUserBySearchColumn(
        QueryOptions<User, GetUserBySearchColumnOptionsData> options,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = (options.Data.SearchTerm ?? "").ToNormalized();
        var searchBySearchTerm = !string.IsNullOrWhiteSpace(normalizedSearchTerm);
        var isSupplier = options.Data.IsSupplier;
        
        var query = context.Users
            .ApplyOptions(options)
            .Where(x => isSupplier == null
                        || (x.UserInfo != null && x.UserInfo.IsSupplier == isSupplier));

        if (searchBySearchTerm)
            query = query
                .Where(x => x.UserInfo != null)
                .Select(x => new
                {
                    Rank = EF.Functions.TrigramsSimilarity(x.UserInfo!.SearchColumn, normalizedSearchTerm) +
                           EF.Functions
                               .TrigramsWordSimilarity(x.UserInfo!.SearchColumn, normalizedSearchTerm) * 0.7,
                    User = x
                })
                .Where(x => x.Rank >= 0.1)
                .OrderByDescending(x => x.Rank)
                .Select(x => x.User);
        else
            query = query.OrderByDescending(x => x.Id);


        return await query
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserDiscounts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Discount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    [SuppressMessage("ReSharper", "EntityFramework.ClientSideDbFunctionCall")]
    public async Task<IReadOnlyList<User>> GetUsersBySimilarityAsync(
        QueryOptions<User, GetUsersBySimilarityOptionsData> options,
        CancellationToken cancellationToken = default)
    {
        var similarityLevel = options.Data.SimilarityLevel >= 1 ? 0.999 : options.Data.SimilarityLevel;
        var query = context.Users
            .ApplyOptions(options);
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

        if (!string.IsNullOrWhiteSpace(options.Data.Name))
        {
            currName = options.Data.Name.Trim().ToUpperInvariant();
            query = query.Where(u => u.UserInfo != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Name.ToUpper(),
                                         currName) > similarityLevel);
            isNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(options.Data.Surname))
        {
            currSurname = options.Data.Surname.Trim().ToUpperInvariant();
            query = query.Where(u => u.UserInfo != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Surname.ToUpper(),
                                         currSurname) > similarityLevel);
            isSurnameIncluded = true;
        }


        if (!string.IsNullOrWhiteSpace(options.Data.Email))
        {
            normalizedEmail = options.Data.Email.ToNormalizedEmail();
            query = query.Where(u => u.UserEmails.Any(e =>
                EF.Functions.TrigramsSimilarity(e.NormalizedEmail,
                    normalizedEmail) > similarityLevel));
            isEmailIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(options.Data.Phone))
        {
            normalizedPhone = options.Data.Phone.Trim().ToNormalizedPhoneNumber();
            query = query.Where(u => u.UserPhones.Any(p =>
                EF.Functions.TrigramsSimilarity(p.NormalizedPhone, normalizedPhone) > similarityLevel));
            isPhoneNumberIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(options.Data.UserName))
        {
            normalizedUserName = options.Data.UserName.Trim().ToUpperInvariant();
            query = query.Where(u =>
                EF.Functions.TrigramsSimilarity(u.NormalizedUserName, normalizedUserName) > similarityLevel);
            isUserNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(options.Data.Description))
        {
            normalizedDescription = options.Data.Description.ToNormalized();
            query = query.Where(u => u.UserInfo!.Description != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Description.ToUpper(),
                                         normalizedDescription) > similarityLevel);
            isDescriptionIncluded = true;
        }

        if (options.Data.Id != null)
            query = query.Where(u => u.Id == options.Data.Id);


        if (options.Data.IsSupplier != null)
            query = query.Where(u => u.UserInfo != null && u.UserInfo.IsSupplier == options.Data.IsSupplier);

        var queryWithScore = query.Select(u => new
            {
                User = u,
                Score =
                    (isNameIncluded ? EF.Functions.TrigramsSimilarity(u.UserInfo!.Name, currName) : 0) +
                    (isSurnameIncluded ? EF.Functions.TrigramsSimilarity(u.UserInfo!.Surname, currSurname) : 0) +
                    (isUserNameIncluded
                        ? EF.Functions.TrigramsSimilarity(u.NormalizedUserName, normalizedUserName)
                        : 0) +
                    (isEmailIncluded
                        ? u.UserEmails
                            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.NormalizedEmail, normalizedEmail))
                            .Select(x => EF.Functions.Greatest(
                                EF.Functions.TrigramsSimilarity(x.NormalizedEmail, normalizedEmail)))
                            .FirstOrDefault()
                        : 0) +
                    (isDescriptionIncluded
                        ? EF.Functions.TrigramsSimilarity(u.UserInfo!.Description!, normalizedDescription)
                        : 0) +
                    (isPhoneNumberIncluded
                        ? u.UserPhones
                            .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.NormalizedPhone, normalizedPhone))
                            .Select(x => EF.Functions.Greatest(
                                EF.Functions.TrigramsSimilarity(x.NormalizedPhone, normalizedPhone)))
                            .FirstOrDefault()
                        : 0)
            })
            .OrderByDescending(x => x.Score);

        var users = await queryWithScore
            .Select(x => x.User)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
        return users;
    }
}