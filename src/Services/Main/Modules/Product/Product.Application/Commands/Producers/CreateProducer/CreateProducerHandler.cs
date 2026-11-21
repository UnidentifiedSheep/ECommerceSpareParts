using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Dtos.Producer;

namespace Main.Modules.Product.Application.Commands.Producers.CreateProducer;

[AutoSave]
[Transactional]
public record CreateProducerCommand(NewProducerDto NewProducer) : ICommand<CreateProducerResult>;

public record CreateProducerResult(int ProducerId);

public class CreateProducerHandler(IUnitOfWork unitOfWork)
	: ICommandHandler<CreateProducerCommand, CreateProducerResult>
{
	public async Task<CreateProducerResult> Handle(
		CreateProducerCommand request,
		CancellationToken cancellationToken)
	{
		var newProducer = request.NewProducer;
		var producer = Producer.Create(newProducer.Name, newProducer.Description);
		await unitOfWork.AddAsync(producer, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateProducerResult(producer.Id);
	}
}
