using Core.Entities;
using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class UserEmailRepository(DContext context) : IUserEmailRepository
{
    public async Task<IEnumerable<UserEmail>> GetUserEmailsAsync(Guid userId, int? limit = null, int? offset = null,
        bool track = true, CancellationToken cancellationToken = default)
    {
        var query = context.UserEmails
            .ConfigureTracking(track)
            .OrderBy(x => x.Id)
            .Where(e => e.UserId == userId);

        if (offset != null)
            query = query.Skip(offset.Value);

        if (limit != null)
            query = query.Take(limit.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserEmail?> GetUserEmailAsync(Guid id, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.UserEmails.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserEmail?> GetUserEmailAsync(string email, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToNormalizedEmail();
        return await context.UserEmails.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetUserByPrimaryMailAsync(string email, bool track = true, CancellationToken cancellationToken = default)
    {
        var userEmail = await context.UserEmails.ConfigureTracking(track)
            .Include(x => x.User)
            .ThenInclude(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == email.ToNormalizedEmail() && 
                                      x.IsPrimary == true, cancellationToken);
        return userEmail?.User;
    }

    public async Task<UserEmail?> GetUserPrimaryEmailAsync(Guid userId, bool track = true, CancellationToken cancellationToken = default)
    => await context.UserEmails.ConfigureTracking(track)
        .FirstOrDefaultAsync(x => x.UserId == userId && x.IsPrimary == true, cancellationToken);
    
    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToNormalizedEmail();
        return await context.UserEmails.AsNoTracking()
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<int> GetUserEmailCountAsync(Guid userId, CancellationToken cancellationToken = default) 
        => await context.UserEmails.AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);

    public async Task<bool> UserHasPrimaryEmailAsync(Guid userId, CancellationToken cancellationToken = default) 
        => await context.UserEmails.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.IsPrimary == true, cancellationToken);

    public async Task<UserEmailSummary?> GetUserEmailSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var summary = await context.UserEmails
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .GroupBy(x => x.UserId)
            .Select(group => new UserEmailSummary
            {
                UserId = group.Key,
                EmailCount = group.Count(),
                PrimaryEmail = group.FirstOrDefault(e => e.IsPrimary)
            })
            .FirstOrDefaultAsync(cancellationToken);
        return summary;
    }

    public async Task<bool> IsEmailExists(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToNormalizedEmail();
        return await context.UserEmails.AsNoTracking()
            .AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<IEnumerable<string>> IsEmailsExists(IEnumerable<string> emails, CancellationToken cancellationToken = default)
    {
        var normalizedEmails = emails.Select(x => x.ToNormalizedEmail())
            .ToHashSet();
        var found = await context.UserEmails.AsNoTracking()
            .Where(x => normalizedEmails.Contains(x.NormalizedEmail))
            .Select(x => x.NormalizedEmail)
            .ToListAsync(cancellationToken);
        return normalizedEmails.Except(found);
    }
}