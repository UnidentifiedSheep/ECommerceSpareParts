using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Extensions;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Models;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserPhoneRepository(DContext context) : IUserPhoneRepository
{
    public async Task<IEnumerable<UserPhone>> GetUserPhonesAsync(Guid userId, int? limit = null, int? offset = null,
        bool track = true, CancellationToken cancellationToken = default)
    {
        var query = context.UserPhones
            .ConfigureTracking(track)
            .OrderBy(x => x.Id)
            .Where(e => e.UserId == userId);

        if (offset != null)
            query = query.Skip(offset.Value);

        if (limit != null)
            query = query.Take(limit.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserPhone?> GetUserPhoneAsync(Guid id, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.UserPhones.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserPhone?> GetUserPhoneAsync(string phone, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phone.ToNormalizedPhoneNumber();
        return await context.UserPhones.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.NormalizedPhone == normalizedPhone, cancellationToken);
    }

    public async Task<UserPhone?> GetUserPrimaryPhoneAsync(Guid userId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.UserPhones.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsPrimary == true, cancellationToken);
    }

    public async Task<bool> IsPhoneTakenAsync(string phone, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phone.ToNormalizedPhoneNumber();
        return await context.UserPhones.AsNoTracking()
            .AnyAsync(x => x.NormalizedPhone == normalizedPhone, cancellationToken);
    }

    public async Task<int> GetUserPhoneCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserPhones.AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<bool> UserHasPrimaryPhoneAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserPhones.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.IsPrimary == true, cancellationToken);
    }

    public async Task<UserPhoneSummary?> GetUserPhoneSummaryAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var summary = await context.UserPhones
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .GroupBy(x => x.UserId)
            .Select(group => new UserPhoneSummary
            {
                UserId = group.Key,
                PhoneCount = group.Count(),
                PrimaryPhone = group.FirstOrDefault(e => e.IsPrimary)
            })
            .FirstOrDefaultAsync(cancellationToken);
        return summary;
    }
}