using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Core.Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserRepository(DContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(Guid userId, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<User, object?>>[] includes)
    {
        var query = context.Users.ConfigureTracking(track);
        foreach (var include in includes)
            query = query.Include(include);
                
        return await query.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
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


    public async Task<UserInfo?> GetUserInfo(Guid id, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.UserInfos
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);
    }


    [SuppressMessage("ReSharper", "EntityFramework.ClientSideDbFunctionCall")]
    public async Task<IEnumerable<User>> GetUserBySearchColumn(string? searchTerm, int page, int viewCount,
        bool? isSupplier = null, bool track = true, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = (searchTerm ?? "").ToNormalized();
        var searchBySearchTerm = !string.IsNullOrWhiteSpace(normalizedSearchTerm);

        var query = context.Users
            .ConfigureTracking(track)
            .Include(x => x.UserInfo)
            .Where(x => isSupplier == null
                        || (x.UserInfo != null && x.UserInfo.IsSupplier == isSupplier));

        if (searchBySearchTerm)
        {
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
        }
        else
            query = query.OrderByDescending(x => x.Id);
        

        return await query
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