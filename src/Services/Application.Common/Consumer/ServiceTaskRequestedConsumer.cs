using Application.Common.Interfaces.Lrt;
using Contracts.Tasks;
using MassTransit;
using NamedObject.Core.Interfaces;

namespace Application.Common.Consumer;

public sealed class ServiceTaskRequestedConsumer(INamedObjectRegistry<ILrtNamedObject> registry) : IConsumer<ServiceTaskRequestedEvent>
{
	public Task Consume(ConsumeContext<ServiceTaskRequestedEvent> context)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(context.Message.TaskSystemName);
		context.CancellationToken.ThrowIfCancellationRequested();
		return registry.GetBySystemName(context.Message.TaskSystemName)
			.ExecuteAsync(Guid.Empty, Guid.Empty, context.CancellationToken);
	}
}
