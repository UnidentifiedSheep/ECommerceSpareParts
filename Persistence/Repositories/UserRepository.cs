using Core.Entities;
using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class UserRepository(DContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(Guid userId, bool track = true, CancellationToken cancellationToken = default) 
        => await context.Users.ConfigureTracking(track)
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task<User?> GetUserByUserNameAsync(string userName, bool track = true, CancellationToken cancellationToken = default)
        => await context.Users.ConfigureTracking(track)
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedUserName == userName.ToNormalized(), cancellationToken);

    public async Task<User?> GetUserByEmailAsync(string email, bool track = true, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToNormalizedEmail();
        var userEmail = await context.UserEmails
            .ConfigureTracking(track)
            .Include(x => x.User)
            .ThenInclude(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        return userEmail?.User;
    }

    public async Task<User?> GetUserByPhoneAsync(string phoneNumber, bool track = true, CancellationToken cancellationToken = default)
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
        => await context.Users.AnyAsync(x => x.Id == id, cancellationToken);
    
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
    
    public async Task<decimal?> GetUsersDiscountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserDiscounts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Discount)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<User>> GetUsersBySimilarityAsync(double similarityLevel, int page,
        int viewCount,
        string? name = null, string? surname = null, string? email = null,
        string? phone = null, string? userName = null, Guid? id = null,
        string? description = null, bool? isSupplier = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        /*similarityLevel = similarityLevel >= 1 ? 0.99999 : similarityLevel;
        var query = context.Users
            .AsNoTracking();
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
            query = query.Where(u => EF.Functions
                                         .TrigramsSimilarity(u.NormalizedEmail!, normalizedEmail) > similarityLevel ||
                                     u.UserMails
                                         .Any(e => EF.Functions
                                                       .TrigramsSimilarity(e.NormalizedEmail, normalizedEmail) >
                                                   similarityLevel ||
                                                   EF.Functions
                                                       .TrigramsSimilarity(e.LocalPart, localPart) > similarityLevel));
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
                    (isUserNameIncluded
                        ? EF.Functions.TrigramsSimilarity(u.NormalizedUserName!, normalizedUserName)
                        : 0) +
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
                    (isDescriptionIncluded
                        ? EF.Functions.TrigramsSimilarity(u.Description!, normalizedDescription)
                        : 0) +
                    (isPhoneNumberIncluded ? EF.Functions.TrigramsSimilarity(u.PhoneNumber!, normalizedPhone) : 0)
            })
            .OrderByDescending(x => x.Score);

        var users = await queryWithScore
            .Select(x => x.User)
            .Skip(page * viewCount)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
        return users;*/
    }
}