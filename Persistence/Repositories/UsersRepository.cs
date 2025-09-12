using Core.Entities;
using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Exceptions.Exceptions.Users;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class UsersRepository(DContext context) : IUsersRepository
{
    public async Task<IEnumerable<AspNetUser>> GetUsersBySimilarityAsync(double similarityLevel, int page, int viewCount, 
        string? name = null, string? surname = null, string? email = null, 
        string? phone = null, string? userName = null, string? id = null,
        string? description = null, bool? isSupplier = null, CancellationToken cancellationToken = default)
    {
        if (similarityLevel is < 0 or > 1)
            throw new SimilarityMustBeBetweenZeroAndOneException(similarityLevel);
        
        similarityLevel = similarityLevel >= 1 ? 0.99999 : similarityLevel;
        var query = context.AspNetUsers
            .AsNoTracking();
        var isNameIncluded = false;
        var isSurnameIncluded = false;
        var isEmailIncluded = false;
        var isPhoneNumberIncluded = false;
        var isUserNameIncluded = false;
        var isDescriptionIncluded = false;

        string currName = "";
        string currSurname = "";
        string normalizedUserName = "";
        var normalizedEmail = "";
        var localPart = "";
        var normalizedPhone = "";
        var normalizedDescription = "";
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            currName = name.Trim().ToUpperInvariant();
            query = query.Where(u => 
                EF.Functions.TrigramsSimilarity(u.Name.ToUpper(), currName) > similarityLevel);
            isNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(surname))
        {
            currSurname = surname.Trim().ToUpperInvariant();
            query = query.Where(u => 
                EF.Functions.TrigramsSimilarity(u.Surname.ToUpper(), currSurname) > similarityLevel);
            isSurnameIncluded = true;
        }

        
        if (!string.IsNullOrWhiteSpace(email))
        {
            normalizedEmail = email.ToUpperInvariant();
            localPart = normalizedEmail.Contains('@') ? normalizedEmail.Split('@')[0] : normalizedEmail;
            query = query.Where(u => ( EF.Functions
                                           .TrigramsSimilarity(u.NormalizedEmail!, normalizedEmail) > similarityLevel) || 
                                     u.UserMails
                                         .Any(e => (EF.Functions
                                             .TrigramsSimilarity(e.NormalizedEmail, normalizedEmail) > similarityLevel) ||
                                                   (EF.Functions
                                                       .TrigramsSimilarity(e.LocalPart, localPart) > similarityLevel)));
            isEmailIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            normalizedPhone = phone.Trim().ToNormalizedPhoneNumber();
            query = query.Where(u => 
                EF.Functions.TrigramsSimilarity(u.PhoneNumber!, normalizedPhone) > similarityLevel);
            isPhoneNumberIncluded = true;
        }
        
        if (!string.IsNullOrWhiteSpace(userName))
        {
            normalizedUserName = userName.Trim().ToUpperInvariant();
            query = query.Where(u => 
                EF.Functions.TrigramsSimilarity(u.NormalizedUserName!, normalizedUserName) > similarityLevel);
            isUserNameIncluded = true;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            normalizedDescription = description.Trim().ToUpperInvariant();
            query = query.Where(u => 
                EF.Functions.TrigramsSimilarity(u.Description!, normalizedDescription) > similarityLevel);
            isDescriptionIncluded = true;
        }
        
        if (!string.IsNullOrWhiteSpace(id))
            query = query.Where(u => u.Id == id);
        
        
        if (isSupplier != null)
            query = query.Where(u => u.IsSupplier == isSupplier);

        var queryWithScore = query.Select(u => new
            {
                User = u,
                Score =
                    (isNameIncluded ? EF.Functions.TrigramsSimilarity(u.Name, currName) : 0) +
                    (isSurnameIncluded ? EF.Functions.TrigramsSimilarity(u.Surname, currSurname) : 0) +
                    (isUserNameIncluded ? EF.Functions.TrigramsSimilarity(u.NormalizedUserName!, normalizedUserName) : 0) +
                    (isEmailIncluded
                        ? EF.Functions.TrigramsSimilarity(u.NormalizedEmail!, normalizedEmail) +
                          u.UserMails
                              .OrderByDescending(x => EF.Functions.Greatest(
                                  EF.Functions.TrigramsSimilarity(x.NormalizedEmail, normalizedEmail),
                                  EF.Functions.TrigramsSimilarity(x.LocalPart, localPart)))
                              .Select(x => EF.Functions.Greatest(
                                  EF.Functions.TrigramsSimilarity(x.NormalizedEmail, normalizedEmail),
                                  EF.Functions.TrigramsSimilarity(x.LocalPart, localPart)))
                              .FirstOrDefault()
                        : 0) +
                    (isDescriptionIncluded ? EF.Functions.TrigramsSimilarity(u.Description!, normalizedDescription) : 0) +
                    (isPhoneNumberIncluded ? EF.Functions.TrigramsSimilarity(u.PhoneNumber!, normalizedPhone) : 0)
            })
            .OrderByDescending(x => x.Score);
        
        var users = await queryWithScore
            .Select(x => x.User)
            .Skip(page * viewCount)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
        return users;
    }

    public async Task<decimal?> GetUsersDiscountAsync(string userId, CancellationToken cancellationToken = default)
        => await context.UserDiscounts.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.Discount)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<string>> UsersExists(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToHashSet();
        var foundIds = await context.AspNetUsers.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id).ToListAsync(cancellationToken);
        return ids.Except(foundIds).ToList();
    }

    public async Task ChangeUsersDiscount(string userId, decimal discount, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlAsync($"""
                                                INSERT INTO user_discounts (user_id, discount)
                                                VALUES ({userId}, {discount})
                                                ON CONFLICT (user_id)
                                                DO UPDATE SET discount = EXCLUDED.discount;
                                                """, cancellationToken);
    }

    public async Task<bool> UserNameTaken(string userName, CancellationToken ct = default)
    {
        return await context.AspNetUsers.AnyAsync(x => x.NormalizedUserName == userName.ToNormalized(), ct);
    }
}