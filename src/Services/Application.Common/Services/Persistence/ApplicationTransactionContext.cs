using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;

namespace Application.Common.Services.Persistence;

public sealed class ApplicationTransactionContext(IUnitOfWork unitOfWork, IRepositoryProvider repositories)
	: IApplicationTransactionContext
{
	public IUnitOfWork UnitOfWork { get; } = unitOfWork;

	public IRepositoryProvider Repositories { get; } = repositories;
}
