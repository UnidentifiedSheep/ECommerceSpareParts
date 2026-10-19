using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Application.Interfaces.Services;
using Main.Entities;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public sealed class RecalculateApproximateOrganizationBalancesLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<RecalculateApproximateOrganizationBalancesLrt> logger) : LrtBase<NoneInputState, NoneInputState>(
	transactionService,
	logger)
{
	private const int BatchSize = 250;

	public override string SystemName => nameof(RecalculateApproximateOrganizationBalancesLrt);

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtOrganizationApproximateBalanceRecalculateNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtOrganizationApproximateBalanceRecalculateDescriptionMessage.Instance;

	protected override async Task DoWork(NoneInputState inputState)
	{
		var lastOrganizationId = Guid.Empty;

		while (true)
		{
			var id = lastOrganizationId;
			var organizationIds = await ExecuteInScopeAsync((context, ct) =>
				context.Repositories.GetForRead<OrganizationFinancialProfile, Guid>()
				.Query
				.Where(x => x.OrganizationId > id)
				.OrderBy(x => x.OrganizationId)
				.Select(x => x.OrganizationId)
				.Take(BatchSize)
				.ToListAsync(ct), CancellationToken);

			if (organizationIds.Count == 0)
				break;

			await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(20, 3),
				async (context, cancellationToken) =>
				{
					await context.GetRequiredService<IBalanceService>().RecalculateApproximateBalancesAsync(
						organizationIds,
						cancellationToken);
					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				},
				CancellationToken);

			lastOrganizationId = organizationIds[^1];
			if (organizationIds.Count < BatchSize)
				break;
		}
	}
}
