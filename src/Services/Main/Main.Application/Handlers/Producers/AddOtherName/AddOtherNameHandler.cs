using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.Producers.AddOtherName;

[Transactional]
public record AddOtherNameCommand(int ProducerId, string OtherName, string WhereUsed) : ICommand<Unit>;

public class AddOtherNameHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<AddOtherNameCommand>
{
    public async Task<Unit> Handle(AddOtherNameCommand request, CancellationToken cancellationToken)
    {
        var model = new ProducersOtherName
        {
            ProducerId = request.ProducerId,
            ProducerOtherName = request.OtherName.Trim(),
            WhereUsed = request.WhereUsed.Trim()
        };

        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}