using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Entities.Product.Exceptions;
using Main.Modules.Product.Application.Dtos.Producer;
using Main.Modules.Product.Application.Interfaces.Persistence;

namespace Main.Modules.Product.Application.Commands.Producers.EditProducer;

[AutoSave]
[Transactional]
public record EditProducerCommand(int ProducerId, PatchProducerDto Producer) : ICommand<EditProducerResult>;

public record EditProducerResult(int ProducerId);

public class EditProducerHandler(IProducerRepository repository)
	: ICommandHandler<EditProducerCommand, EditProducerResult>
{
	public async Task<EditProducerResult> Handle(
		EditProducerCommand request,
		CancellationToken cancellationToken)
	{
		var producer = await repository.GetById(request.ProducerId, cancellationToken) ??
			throw new ProducerNotFoundException(request.ProducerId);

		var patch = request.Producer;
		patch.Name.Apply(producer.SetName);
		patch.Description.Apply(producer.SetDescription);

		return new EditProducerResult(producer.Id);
	}
}
