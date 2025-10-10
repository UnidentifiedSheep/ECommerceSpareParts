using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Producers;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Producers.DeleteOtherName;

[Transactional]
public record DeleteOtherNameCommand(int ProducerId, string OtherName, string? Usage) : ICommand;

public class DeleteOtherNameHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteOtherNameCommand>
{
    public async Task<Unit> Handle(DeleteOtherNameCommand request, CancellationToken cancellationToken)
    {
        var producerOtherName = await producerRepository.GetOtherName(request.ProducerId, request.OtherName,
                                    request.Usage, true, cancellationToken)
                                ?? throw new ProducersOtherNameNotFoundException(request.OtherName);

        unitOfWork.Remove(producerOtherName);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}