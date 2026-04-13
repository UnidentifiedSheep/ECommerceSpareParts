using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.Producers.AddOtherName;

[AutoSave]
[Transactional]
public record AddOtherNameCommand(int ProducerId, string OtherName, string WhereUsed) : ICommand<Unit>;

public class AddOtherNameHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddOtherNameCommand>
{
    public async Task<Unit> Handle(AddOtherNameCommand request, CancellationToken cancellationToken)
    {
        var model = ProducerOtherName.Create(request.ProducerId, request.OtherName, request.WhereUsed);
        await unitOfWork.AddAsync(model, cancellationToken);
        return Unit.Value;
    }
}