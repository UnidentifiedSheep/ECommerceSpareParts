using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Producers.AddOtherName;

[Transactional]
public record AddOtherNameCommand(int ProducerId, string OtherName, string? WhereUsed) : ICommand<Unit>;

public class AddOtherNameHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator)
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
        var plan = new ValidationPlan().EnsureProducerExists(producerId);
        await dbValidator.Validate(plan, true, true, cancellationToken);
        if (await producerRepository.OtherNameIsTaken(otherName, producerId, whereUsed, cancellationToken))
            throw new SameProducerOtherNameExistsException();
    }
}