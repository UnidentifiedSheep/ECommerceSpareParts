using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using MediatR;

namespace Main.Application.Handlers.ProducerAliases;

[AutoSave]
[Transactional]
public record DeleteAliasCommand(int ProducerId, string Alias) : ICommand;

public class DeleteAliasHandler(
    IRepository<ProducerAlias, string> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAliasCommand>
{
    public async Task<Unit> Handle(DeleteAliasCommand request, CancellationToken cancellationToken)
    {
        var producerAlias = await repository.GetById(
                                Producer.ToNormalizedName(request.Alias),
                                cancellationToken)
                            ?? throw new ProducersAliasNotFoundException(request.Alias);

        if (producerAlias.ProducerId != request.ProducerId)
            throw new ProducersAliasNotFoundException(request.Alias);

        unitOfWork.Remove(producerAlias);
        return Unit.Value;
    }
}