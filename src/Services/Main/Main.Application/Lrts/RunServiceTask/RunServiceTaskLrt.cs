using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Contracts.Tasks;
using Locan.Core.Interfaces;
using Main.Entities;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.RunServiceTask;

public sealed class RunServiceTaskLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<RunServiceTaskLrt> logger) : LrtBase<RunServiceTaskInputState, NoneInputState>(
	transactionService,
	logger)
{
	public const string Name = nameof(RunServiceTaskLrt);

	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtServiceTaskRunNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => LrtServiceTaskRunDescriptionMessage.Instance;

	protected override Task DoWork(RunServiceTaskInputState inputState) =>
		ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			(context, _) =>
			{
				context.GetRequiredService<IIntegrationEventScope>().Add(new ServiceTaskRequestedEvent
				{
					TaskSystemName = inputState.TaskSystemName
				}, inputState.ServiceName);
				return Task.CompletedTask;
			},
			CancellationToken);
}
