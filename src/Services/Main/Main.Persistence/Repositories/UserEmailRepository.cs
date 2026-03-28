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
    private static readonly PageableQueryOptions<UserEmail> DefaultQueryOptions = 
        new PageableQueryOptions<UserEmail>().WithOrderBy(x => x.Id); 
    
    public async Task<IReadOnlyList<UserEmail>> GetUserEmailsAsync(
        Guid userId,
        PageableQueryOptions<UserEmail>? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= DefaultQueryOptions;
        
        return await context.UserEmails
            .ApplyOptions(options)
            .Where(e => e.UserId == userId)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserEmail?> GetPrimaryUserEmail(
        string email,
        QueryOptions<UserEmail>? options = null,
        CancellationToken cancellationToken = default)
    {
        var userEmail = await context.UserEmails
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == email.ToNormalizedEmail() &&
                                      x.IsPrimary, cancellationToken);
        return userEmail;
    }
}