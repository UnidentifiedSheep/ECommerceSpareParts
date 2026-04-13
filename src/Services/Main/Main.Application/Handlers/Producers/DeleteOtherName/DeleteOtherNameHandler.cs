using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Producers;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.Producers.DeleteOtherName;

[AutoSave]
[Transactional]
public record DeleteOtherNameCommand(int ProducerId, string OtherName, string Usage) : ICommand;

public class DeleteOtherNameHandler(
    IRepository<ProducerOtherName, ProducerOtherNameKey> repository, 
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteOtherNameCommand>
{
    public async Task<Unit> Handle(DeleteOtherNameCommand request, CancellationToken cancellationToken)
    {
        var key = new ProducerOtherNameKey(request.ProducerId, request.OtherName, request.Usage);
        var producerOtherName = await repository.GetById(key, cancellationToken)
                                ?? throw new ProducersOtherNameNotFoundException(request.OtherName);

        unitOfWork.Remove(producerOtherName);
        return Unit.Value;
    }
}