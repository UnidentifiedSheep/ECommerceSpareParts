using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;

namespace Application.Common.Services.Persistence;

public sealed class ApplicationTransactionContext(
	IUnitOfWork unitOfWork,
	IRepositoryProvider repositories,
	IIntegrationEventScope integrationEventScope)
	: IApplicationTransactionContext
{
	public IUnitOfWork UnitOfWork => unitOfWork;

	public IRepositoryProvider Repositories => repositories;

	public IIntegrationEventScope IntegrationEventScope => integrationEventScope;
}
