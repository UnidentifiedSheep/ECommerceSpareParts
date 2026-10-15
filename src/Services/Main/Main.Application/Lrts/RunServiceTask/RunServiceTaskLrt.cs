using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Contracts.Tasks;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.RunServiceTask;

public sealed class RunServiceTaskLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IIntegrationEventScope integrationEventScope,
	ILogger<RunServiceTaskLrt> logger) : LrtBase<RunServiceTaskInputState, NoneInputState>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{
	public override string SystemName => nameof(RunServiceTaskLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtServiceTaskRunNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => LrtServiceTaskRunDescriptionMessage.Instance;

	protected override Task DoWork(RunServiceTaskInputState inputState) =>
		TransactionService.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			(_, _) =>
			{
				integrationEventScope.Add(new ServiceTaskRequestedEvent
				{
					TaskSystemName = inputState.TaskSystemName
				}, inputState.ServiceName);
				return Task.CompletedTask;
			},
			CancellationToken);
}
