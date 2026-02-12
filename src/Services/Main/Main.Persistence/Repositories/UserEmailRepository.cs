using Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

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

    public async Task<User?> GetUserByPrimaryMailAsync(string email, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await context.UserEmails.ConfigureTracking(track)
            .Include(x => x.User)
            .ThenInclude(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == email.ToNormalizedEmail() &&
                                      x.IsPrimary, cancellationToken);
        return userEmail?.User;
    }
}