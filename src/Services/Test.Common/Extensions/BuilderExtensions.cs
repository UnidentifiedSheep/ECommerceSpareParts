using Microsoft.EntityFrameworkCore;
using Test.Common.Interfaces;

namespace Test.Common.Extensions;

public static class BuilderExtensions
{
    public static async Task<T> BuildAndAddToDb<T>(
        this IBuilder<T> builder,
        DbContext context)
    {
        var entity = builder.Build();
        ArgumentNullException.ThrowIfNull(entity);

        await context.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public static async Task<IReadOnlyCollection<T>> BuildManyAndAddToDb<T>(
        this IBuilder<T> builder,
        DbContext context,
        int count) where T : class
    {
        var entities = builder.BuildMany(count);
        await context.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        return entities;
    }

    public static async Task<IReadOnlyCollection<T>> BuildManyCombinedAndAddToDb<T>(
        DbContext context,
        int count,
        bool saveChanges = true,
        params IBuilder<T>[] builders) where T : class
    {
        var entities = new List<T>();
        foreach (var builder in builders)
            entities.AddRange(builder.BuildMany(count));
        
        await context.AddRangeAsync(entities);

        if (saveChanges)
            await context.SaveChangesAsync();
        return entities;
    }

    public static Task<IReadOnlyCollection<T>> BuildManyCombinedAndAddToDb<T>(
        this IEnumerable<IBuilder<T>> builders,
        DbContext context,
        int count,
        bool saveChanges = true) where T : class
    {
        return BuildManyCombinedAndAddToDb(context, count, saveChanges, builders.ToArray());
    }
}