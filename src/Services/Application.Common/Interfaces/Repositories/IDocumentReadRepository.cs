using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IDocumentReadRepository<TEntity, TKey> 
    : IReadRepository<TEntity, TKey> 
    where TKey : notnull 
    where TEntity : Entity<TEntity, TKey>
{
    
}