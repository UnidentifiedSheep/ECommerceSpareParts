using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IRepositoryProvider
{
	IRepository<TEntity, TKey> Get<TEntity, TKey>()
		where TEntity : Entity<TEntity, TKey> where TKey : notnull;

	IReadRepository<TEntity, TKey> GetForRead<TEntity, TKey>()
		where TEntity : Entity<TEntity, TKey> where TKey : notnull;

	TRepository Get<TRepository>() where TRepository : class, IRepository;
}
