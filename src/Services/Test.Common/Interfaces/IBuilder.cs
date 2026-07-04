namespace Tests.Interfaces;

public interface IBuilder<TEntity>
{
    TEntity Build();
    IReadOnlyCollection<TEntity> BuildMany(int count);
}