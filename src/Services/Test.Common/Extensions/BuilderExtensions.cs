using Abstractions.Interfaces.Services;
using Test.Common.Interfaces;

namespace Test.Common.Extensions;

public static class BuilderExtensions
{
    public static async Task<T> BuildAndAddToDb<T>(
        this IBuilder<T> builder, 
        IUnitOfWork unitOfWork)
    {
        var entity = builder.Build();
        await unitOfWork.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();
        return entity;
    }
    
    public static async Task<IReadOnlyCollection<T>> BuildManyAndAddToDb<T>(
        this IBuilder<T> builder, 
        IUnitOfWork unitOfWork,
        int count) where T : class
    {
        var entities = builder.BuildMany(count);
        await unitOfWork.AddRangeAsync(entities);
        await unitOfWork.SaveChangesAsync();
        return entities;
    }
}