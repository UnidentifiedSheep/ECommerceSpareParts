using System.Diagnostics.CodeAnalysis;
using Core.Entities;
using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class UserRepository(DContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(Guid userId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Users.ConfigureTracking(track)
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetUserByUserNameAsync(string userName, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Users.ConfigureTracking(track)
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedUserName == userName.ToNormalized(), cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToNormalizedEmail();
        var userEmail = await context.UserEmails
            .ConfigureTracking(track)
            .Include(x => x.User)
            .ThenInclude(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        return userEmail?.User;
    }

    public async Task<User?> GetUserByPhoneAsync(string phoneNumber, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phoneNumber.ToNormalizedPhoneNumber();
        var userPhone = await context.UserPhones.ConfigureTracking(track)
            .Include(x => x.User)
            .ThenInclude(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedPhone == normalizedPhone, cancellationToken);
        return userPhone?.User;
    }

    public async Task<bool> IsUserNameTakenAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.ToNormalized();
        return await context.Users.AnyAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public async Task<bool> UserExists(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task ChangeUsersDiscount(Guid userId, decimal discount,
        CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlAsync($"""
                                                INSERT INTO user_discounts (user_id, discount)
                                                VALUES ({userId}, {discount})
                                                ON CONFLICT (user_id)
                                                DO UPDATE SET discount = EXCLUDED.discount;
                                                """, cancellationToken);
    }

    public async Task<List<Guid>> UsersExists(IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToHashSet();
        var foundIds = await context.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id).ToListAsync(cancellationToken);
        return ids.Except(foundIds).ToList();
    }

    [SuppressMessage("ReSharper", "EntityFramework.ClientSideDbFunctionCall")]
    public async Task<IEnumerable<User>> GetUserBySearchColumn(string? searchTerm, int page, int viewCount,
        bool? isSupplier = null, bool track = true, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm ?? "";
        normalizedSearchTerm = normalizedSearchTerm.ToNormalized();

        var searchBySearchTerm = !string.IsNullOrWhiteSpace(normalizedSearchTerm);

        var query = context.Users.ConfigureTracking(track)
            .Include(x => x.UserInfo)
            .Where(x => isSupplier == null || (x.UserInfo != null && x.UserInfo.IsSupplier == isSupplier))
            .Where(x => !searchBySearchTerm ||
                        (x.UserInfo != null && EF.Functions.TrigramsSimilarity(x.UserInfo!.SearchColumn,
                            normalizedSearchTerm) >= 0.1))
            .Select(x => new
            {
                Rank = searchBySearchTerm
                    ? EF.Functions.TrigramsSimilarity(x.UserInfo!.SearchColumn, normalizedSearchTerm)
                    : 0,
                User = x
            });

        var orderQuery = searchBySearchTerm
            ? query.OrderByDescending(x => x.Rank)
            : query.OrderByDescending(x => x.User.Id);

        return await orderQuery
            .Select(x => x.User)
            .Skip(page * viewCount)
            .Take(viewCount)
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
    public async Task<IEnumerable<User>> GetUsersBySimilarityAsync(double similarityLevel, int page, int viewCount,
        string? name = null, string? surname = null, string? email = null,
        string? phone = null, string? userName = null, Guid? id = null,
        string? description = null, bool? isSupplier = null, bool track = true,
        CancellationToken cancellationToken = default)
    {
        similarityLevel = similarityLevel >= 1 ? 0.999 : similarityLevel;
        var query = context.Users
            .Include(x => x.UserInfo)
            .ConfigureTracking(track);
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

        if (!string.IsNullOrWhiteSpace(name))
        {
            currName = name.Trim().ToUpperInvariant();
            query = query.Where(u => u.UserInfo != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Name.ToUpper(),
                                         currName) > similarityLevel);
            isNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(surname))
        {
            currSurname = surname.Trim().ToUpperInvariant();
            query = query.Where(u => u.UserInfo != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Surname.ToUpper(),
                                         currSurname) > similarityLevel);
            isSurnameIncluded = true;
        }


        if (!string.IsNullOrWhiteSpace(email))
        {
            normalizedEmail = email.ToNormalizedEmail();
            query = query.Where(u => u.UserEmails.Any(e =>
                EF.Functions.TrigramsSimilarity(e.NormalizedEmail,
                    normalizedEmail) > similarityLevel));
            isEmailIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            normalizedPhone = phone.Trim().ToNormalizedPhoneNumber();
            query = query.Where(u => u.UserPhones.Any(p =>
                EF.Functions.TrigramsSimilarity(p.NormalizedPhone, normalizedPhone) > similarityLevel));
            isPhoneNumberIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            normalizedUserName = userName.Trim().ToUpperInvariant();
            query = query.Where(u =>
                EF.Functions.TrigramsSimilarity(u.NormalizedUserName, normalizedUserName) > similarityLevel);
            isUserNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            normalizedDescription = description.ToNormalized();
            query = query.Where(u => u.UserInfo!.Description != null &&
                                     EF.Functions.TrigramsSimilarity(u.UserInfo.Description.ToUpper(),
                                         normalizedDescription) > similarityLevel);
            isDescriptionIncluded = true;
        }

        if (id != null)
            query = query.Where(u => u.Id == id);


        if (isSupplier != null)
            query = query.Where(u => u.UserInfo != null && u.UserInfo.IsSupplier == isSupplier);

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
            .Skip(page * viewCount)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
        return users;
    }
}