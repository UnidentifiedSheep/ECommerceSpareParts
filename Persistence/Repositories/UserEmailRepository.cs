using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class UserEmailRepository(DContext context) : IUserEmailRepository
{
    public async Task<bool> EmailTaken(string email, CancellationToken ct = default)
    {
        var exists = await context.UserMails.AsNoTracking().AnyAsync(x => x.NormalizedEmail == email.ToNormalized(), ct);
        return exists;
    }
}