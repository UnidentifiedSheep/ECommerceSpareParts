using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Producer;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.Producers.AddAlias;

[AutoSave]
[Transactional]
public record AddAliasCommand(
    int ProducerId,
    string Alias
) : ICommand<Unit>;

public class AddAliasHandler(
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope
) : ICommandHandler<AddAliasCommand>
{
    public async Task<Unit> Handle(AddAliasCommand request, CancellationToken cancellationToken)
    {
        var model = ProducerAlias.Create(
            request.ProducerId,
            request.Alias);
        await unitOfWork.AddAsync(model, cancellationToken);
        integrationEventScope.Add(
            new ProducerUpdatedEvent
            {
                Id = request.ProducerId
            });
        return Unit.Value;
    }
}