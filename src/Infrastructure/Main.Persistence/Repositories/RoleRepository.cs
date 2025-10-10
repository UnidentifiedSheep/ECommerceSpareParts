using System.Diagnostics.CodeAnalysis;
using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class RoleRepository(DContext context) : IRoleRepository
{
    public async Task<Role?> GetRoleAsync(Guid id, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.Roles.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Role?> GetRoleAsync(string name, bool track = true, CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = name.ToNormalized();
        return await context.Roles.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesAsync(IEnumerable<string> names, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var normalized = names.Select(x => x.ToNormalized()).ToHashSet();
        return await context.Roles.ConfigureTracking(track)
            .Where(x => normalized.Contains(x.NormalizedName))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Roles.AsNoTracking()
            .AnyAsync(x => x.NormalizedName == name.ToNormalized(), cancellationToken);
    }

    public async Task<bool> RoleExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Roles.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> RolesExistsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var set = ids.ToHashSet();
        var foundRoles = await context.Roles.AsNoTracking()
            .Where(x => set.Contains(x.Id))
            .Select(x => x.Id).ToListAsync(cancellationToken);
        return set.Except(foundRoles);
    }

    public async Task<IEnumerable<string>> RolesExistsAsync(IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var set = roleNames
            .Select(x => x.ToNormalized())
            .ToHashSet();
        var foundRoles = await context.Roles.AsNoTracking()
            .Where(x => set.Contains(x.NormalizedName))
            .Select(x => x.NormalizedName)
            .ToListAsync(cancellationToken);
        return set.Except(foundRoles);
    }

    [SuppressMessage("ReSharper", "EntityFramework.ClientSideDbFunctionCall")]
    public async Task<IEnumerable<Role>> SearchRoles(string? searchTerm, int page, int limit, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Roles.ConfigureTracking(track)
            .Where(x => EF.Functions.ILike(x.NormalizedName, $"%{searchTerm}%"))
            .Select(x => new { Role = x, Rank = EF.Functions.TrigramsSimilarity(x.NormalizedName, $"%{searchTerm}%") })
            .OrderByDescending(x => x.Rank)
            .Select(x => x.Role)
            .Skip(page * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}