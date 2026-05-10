using System.Data;
using Abstractions.Interfaces.Services;
using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Entities;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 2, 20)]
public record UpsertPurchaseFactCommand(PurchaseFactUpsertDto PurchaseFact) : ICommand;

public class UpsertPurchaseFactHandler(
    IRepository<PurchasesFact, Guid> factRepository,
    ILogger<UpsertPurchaseFactCommand> logger,
    IUnitOfWork unitOfWork) : ICommandHandler<UpsertPurchaseFactCommand>
{
    public async Task<Unit> Handle(UpsertPurchaseFactCommand request, CancellationToken cancellationToken)
    {
        var dto = request.PurchaseFact;
        var dbFact = await factRepository.GetById(
            request.PurchaseFact.Id,
            cancellationToken);
        
        if (dto.LastUpdatedAt <= dbFact?.ProcessedAt)
        {
            logger.LogWarning(
                "Purchase fact Id: {id} upsert skipped, because current record is newer than incoming." +
                "Last processed at: {lastProcessedAt}. Incoming creation date time: {creationDate}",
                dto.Id,
                dbFact.ProcessedAt,
                dto.LastUpdatedAt);

            return Unit.Value;
        }
        
        
        var contents = dto.Content.Select(x =>
            PurchaseContent.Create(x.Id, dto.Id, x.ArticleId, x.Price, x.Count));

        if (dbFact == null)
        {
            dbFact = PurchasesFact.Create(
                dto.Id,
                dto.CurrencyId,
                dto.SupplierId,
                dto.CreatedAt,
                dto.LastUpdatedAt,
                contents);
            await unitOfWork.AddAsync(dbFact, cancellationToken);
            return Unit.Value;
        }
        
        unitOfWork.RemoveRange(dbFact.PurchaseContents);
        dbFact.Update(dto.CurrencyId, dto.SupplierId, dto.CreatedAt, dto.LastUpdatedAt, contents);
        
        return Unit.Value;
    }
}