using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Main.Application.Interfaces.Services;
using Main.Entities.Organization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public sealed class RecalculateApproximateOrganizationBalancesLrt(
    IRepository<Job, Guid> jobRepository,
    IReadRepository<OrganizationFinancialProfile, Guid> financialProfileRepository,
    IBalanceService balanceService,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ILogger<RecalculateApproximateOrganizationBalancesLrt> logger
) : LrtBase<NoneInputState, NoneInputState>(
    jobRepository,
    unitOfWork,
    publisher,
    transactionService,
    logger)
{
    private const int BatchSize = 250;
    public override string SystemName => nameof(RecalculateApproximateOrganizationBalancesLrt);
    public override string NameLocalizationKey => "lrt.organization.approximate.balance.recalculate.name";
    public override string DescriptionLocalizationKey =>
        "lrt.organization.approximate.balance.recalculate.description";

    protected override async Task DoWork()
    {
        var lastOrganizationId = Guid.Empty;

        while (true)
        {
            var id = lastOrganizationId;
            var organizationIds = await financialProfileRepository.Query
                .Where(x => x.OrganizationId > id)
                .OrderBy(x => x.OrganizationId)
                .Select(x => x.OrganizationId)
                .Take(BatchSize)
                .ToListAsync(CancellationToken);

            if (organizationIds.Count == 0) break;

            await TransactionService.ExecuteAsync(
                TransactionalAttribute.ReadCommitted(20, 3),
                async (context, cancellationToken) =>
                {
                    await balanceService.RecalculateApproximateBalancesAsync(
                        organizationIds,
                        cancellationToken);
                    await context.UnitOfWork.SaveChangesAsync(cancellationToken);
                },
                CancellationToken);

            lastOrganizationId = organizationIds[^1];
            if (organizationIds.Count < BatchSize) break;
        }
    }
}
