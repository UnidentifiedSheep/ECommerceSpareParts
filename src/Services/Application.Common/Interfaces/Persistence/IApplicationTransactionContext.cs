using Application.Common.Interfaces.Repositories;

namespace Application.Common.Interfaces.Persistence;

public interface IApplicationTransactionContext
{
	IUnitOfWork UnitOfWork { get; }

	IRepositoryProvider Repositories { get; }
}
