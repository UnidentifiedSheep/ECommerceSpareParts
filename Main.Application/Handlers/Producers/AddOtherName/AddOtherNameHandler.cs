using Main.Application.Extensions;
using Core.Attributes;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Main.Application.Interfaces;
using MediatR;

namespace Main.Application.Handlers.Producers.AddOtherName;

[Transactional]
public record AddOtherNameCommand(int ProducerId, string OtherName, string? WhereUsed) : ICommand<Unit>;

public class AddOtherNameHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<AddOtherNameCommand>
{
    public async Task<Unit> Handle(AddOtherNameCommand request, CancellationToken cancellationToken)
    {
        var producerId = request.ProducerId;
        var otherName = request.OtherName.Trim();
        var usage = request.WhereUsed?.Trim();

        await ValidateData(producerId, otherName, usage, cancellationToken);

        var model = new ProducersOtherName
        {
            ProducerId = producerId,
            ProducerOtherName = otherName,
            WhereUsed = usage
        };

        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(int producerId, string otherName, string? whereUsed,
        CancellationToken cancellationToken = default)
    {
        await producerRepository.EnsureProducersExists([producerId], cancellationToken);
        if (await producerRepository.OtherNameIsTaken(otherName, producerId, whereUsed, cancellationToken))
            throw new SameProducerOtherNameExistsException();
    }
}