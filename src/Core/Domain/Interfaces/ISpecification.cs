namespace Domain.Interfaces;

public interface ISpecification<TEntity, in TKey> where TEntity : Entity<TEntity, TKey>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}