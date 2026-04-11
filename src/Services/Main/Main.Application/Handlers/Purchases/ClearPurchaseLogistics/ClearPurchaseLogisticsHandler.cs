using Abstractions.Interfaces.Services;
using Abstractions.Models.Command;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Purchase;

namespace Main.Application.Handlers.Purchases.ClearPurchaseLogistics;

[Transactional]
public record ClearPurchaseLogisticsCommand(string PurchaseId, CommandOptions? Options = null)
    : ICommand<ClearPurchaseLogisticsResult>;

public record ClearPurchaseLogisticsResult(PurchaseLogistic? PurchaseLogistic, List<PurchaseContent> Contents);

public class ClearPurchaseLogisticsHandler(
    IPurchaseLogisticsRepository logisticsRepository,
    IUnitOfWork unitOfWork,
    IPurchaseRepository purchaseRepository)
    : ICommandHandler<ClearPurchaseLogisticsCommand, ClearPurchaseLogisticsResult>
{
    public async Task<ClearPurchaseLogisticsResult> Handle(
        ClearPurchaseLogisticsCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseLogistics = await logisticsRepository.GetPurchaseLogistics(
            new QueryOptions<PurchaseLogistic, string>() { Data = request.PurchaseId }
                .WithTracking()
                .WithInclude(x => x.Transaction), 
            cancellationToken);

        var contents = (await purchaseRepository
                .GetPurchaseContent(
                    new QueryOptions<PurchaseContent, string>() { Data = request.PurchaseId }
                        .WithTracking()
                        .WithInclude(x => x.PurchaseContentLogistic), 
                    cancellationToken))
            .ToList();

        foreach (var content in contents)
        {
            if (content.PurchaseContentLogistic == null) continue;
            unitOfWork.Remove(content.PurchaseContentLogistic);
            content.PurchaseContentLogistic = null;
        }

        if (purchaseLogistics != null) unitOfWork.Remove(purchaseLogistics);

        if (request.Options == null || request.Options.SaveChanges)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClearPurchaseLogisticsResult(purchaseLogistics, contents);
    }
}