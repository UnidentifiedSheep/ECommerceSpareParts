using Abstractions.Models.Repository;
using Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class UserEmailRepository(DContext context) : IUserEmailRepository
{
    public async Task<IReadOnlyList<UserEmail>> GetUserEmailsAsync(
        QueryOptions<UserEmail, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.UserEmails
            .ApplyOptions(options)
            .Where(e => e.UserId == options.Data)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserEmail?> GetUserEmailByPrimary(
        QueryOptions<UserEmail, string> options,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await context.UserEmails
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => 
                x.NormalizedEmail == options.Data.ToNormalizedEmail() &&
                x.IsPrimary, cancellationToken);
        return userEmail;
    }
}